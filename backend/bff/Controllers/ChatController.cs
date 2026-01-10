using Microsoft.AspNetCore.Mvc;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly GeminiService _geminiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(GeminiService geminiService, ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message is required." });

        try
        {
            var reply = await _geminiService.ChatAsync(request.Message);
            if (reply.StartsWith("Error:")) 
            {
                 // Return the error message from the service
                 return StatusCode(500, new { error = reply });
            }
            return Ok(new { reply });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat");
            return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
        }
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}
