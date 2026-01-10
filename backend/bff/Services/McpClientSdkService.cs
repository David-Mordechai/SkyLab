using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace SkyLab.Backend.Services;

public class McpClientSdkService
{
    private readonly ILogger<McpClientSdkService> _logger;
    private McpClient? _client;
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

    public McpClientSdkService(ILogger<McpClientSdkService> logger)
    {
        _logger = logger;
    }

    public bool IsConnected => _client != null;

    public async Task ConnectAsync(string url)
    {
        if (_client != null) return;

        await _connectionLock.WaitAsync();
        try
        {
            if (_client != null) return;

            _logger.LogInformation($"Connecting to MCP Server at {url}...");
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var transport = new SseClientTransport(new Uri(url), httpClient);
            
            var options = new McpClientOptions
            {
                 ClientInfo = new Implementation { Name = "SkyLab BFF", Version = "1.0.0" }
            };

            _client = await McpClient.CreateAsync(transport, options);
            _logger.LogInformation("Successfully connected to MCP Server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to connect to MCP Server at {url}");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<List<McpClientTool>> GetToolsAsync()
    {
        if (_client == null) 
        {
            _logger.LogWarning("GetToolsAsync called but client is not connected.");
            return new List<McpClientTool>();
        }
        
        try
        {
            var result = await _client.ListToolsAsync();
            var tools = result.ToList();
            _logger.LogInformation($"Retrieved {tools.Count} tools from MCP Server.");
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tools from MCP Server.");
            return new List<McpClientTool>();
        }
    }

    public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object?> arguments)
    {
        if (_client == null) throw new InvalidOperationException("Not connected");

        _logger.LogInformation($"Calling MCP tool: {name}");
        return await _client.CallToolAsync(name, arguments);
    }
}