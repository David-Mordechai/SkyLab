using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace SkyLab.Backend.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly McpClientSdkService _mcpClient;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, McpClientSdkService mcpClient, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _mcpClient = mcpClient;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string userMessage)
    {
        var apiKey = _configuration["GEMINI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey)) return "Error: GEMINI_API_KEY is not configured.";

        await EnsureMcpConnected();

        var geminiTools = await GetAndMapTools();
        var initialResponse = await CallGeminiWithTools(apiKey, userMessage, geminiTools);

        return await HandleGeminiResponse(apiKey, userMessage, geminiTools, initialResponse);
    }

    private async Task EnsureMcpConnected()
    {
        if (_mcpClient.IsConnected) return;

        try 
        {
            await _mcpClient.ConnectAsync("http://127.0.0.1:3001/sse");
        }
        catch (Exception ex)
        {
            _logger.LogError($"MCP Connection failed: {ex.Message}");
            // We throw here because without MCP we can't do mission control properly in this context
            throw new Exception("Could not connect to Mission Control (MCP).", ex);
        }
    }

    private async Task<List<object>> GetAndMapTools()
    {
        var mcpTools = await _mcpClient.GetToolsAsync();
        _logger.LogInformation($"Mapping {mcpTools.Count} MCP tools to Gemini format.");
        var geminiTools = new List<object>();

        foreach (var tool in mcpTools)
        {
            var cleanSchema = ExtractAndCleanSchema(tool);
            if (cleanSchema != null)
            {
                _logger.LogInformation($"Mapping tool: {tool.Name}");
                geminiTools.Add(new 
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = cleanSchema 
                });
            }
            else
            {
                _logger.LogWarning($"Skipping tool {tool.Name} due to missing schema.");
            }
        }
        return geminiTools;
    }

    private JsonNode? ExtractAndCleanSchema(McpClientTool tool)
    {
        // SDK serialization hack to access properties dynamically
        var toolJson = JsonSerializer.Serialize(tool);
        var toolNode = JsonNode.Parse(toolJson);
        if (toolNode == null) return null;

        var schemaNode = toolNode["JsonSchema"] ?? toolNode["inputSchema"] ?? toolNode["parameters"];
        if (schemaNode == null) return null;

        var cleanSchema = JsonNode.Parse(schemaNode.ToJsonString());
        CleanSchemaRecursive(cleanSchema);
        return cleanSchema;
    }

    private void CleanSchemaRecursive(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("$schema")) obj.Remove("$schema");
            foreach (var property in obj) CleanSchemaRecursive(property.Value);
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr) CleanSchemaRecursive(item);
        }
    }

    private async Task<JsonNode> CallGeminiWithTools(string apiKey, string userMessage, List<object> tools)
    {
        var requestBody = new
        {
            contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
            tools = new[] { new { function_declarations = tools } }
        };
        return await PostToGemini(apiKey, requestBody);
    }

    private async Task<string> HandleGeminiResponse(string apiKey, string userMessage, List<object> tools, JsonNode response)
    {
        var content = response["candidates"]?[0]?["content"];
        if (content == null) return "Error: No response.";

        var parts = content["parts"]?.AsArray();
        if (parts == null) return "Error: Empty response.";

        foreach (var part in parts)
        {
            if (part["functionCall"] is JsonObject funcCall)
            {
                return await ExecuteToolAndFollowUp(apiKey, userMessage, tools, part, funcCall);
            }
        }

        return parts[0]?["text"]?.ToString() ?? "";
    }

    private async Task<string> ExecuteToolAndFollowUp(string apiKey, string userMessage, List<object> tools, JsonNode modelPart, JsonObject funcCall)
    {
        var funcName = funcCall["name"]?.ToString();
        var args = funcCall["args"];
        
        _logger.LogInformation($"Executing Tool: {funcName}");
        
        string toolResult = "Error";
        if (!string.IsNullOrEmpty(funcName) && args != null)
        {
            try
            {
                var dictArgs = JsonSerializer.Deserialize<Dictionary<string, object?>>(args.ToJsonString());
                var resultObj = await _mcpClient.CallToolAsync(funcName, dictArgs ?? new Dictionary<string, object?>());
                
                toolResult = resultObj.Content != null && resultObj.Content.Any() 
                    ? JsonSerializer.Serialize(resultObj.Content) 
                    : "Success";
            }
            catch (Exception ex)
            {
                toolResult = $"Tool Execution Error: {ex.Message}";
            }
        }

        // Follow-up with Gemini
        var followUpBody = new
        {
            contents = new List<object>
            {
                new { role = "user", parts = new[] { new { text = userMessage } } },
                new { role = "model", parts = new[] { modelPart } },
                new { role = "function", parts = new[] { 
                    new { 
                        functionResponse = new {
                            name = funcName,
                            response = new { result = toolResult }
                        }
                    } 
                }}
            },
            tools = new[] { new { function_declarations = tools } }
        };

        var finalResponse = await PostToGemini(apiKey, followUpBody);
        return finalResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "Mission Updated.";
    }

    private async Task<JsonNode> PostToGemini(string apiKey, object body)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
        var jsonBody = JsonSerializer.Serialize(body);
        
        var response = await _httpClient.PostAsync(url, new StringContent(jsonBody, Encoding.UTF8, "application/json"));
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Gemini API Error: {jsonResponse}");
            throw new Exception($"Gemini Error {response.StatusCode}");
        }

        return JsonNode.Parse(jsonResponse) ?? new JsonObject();
    }
}