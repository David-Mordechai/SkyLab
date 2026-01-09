using Microsoft.AspNetCore.Mvc;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Controllers;

[ApiController]
[Route("api/mission")]
public class MissionController : ControllerBase
{
    private readonly FlightStateService _flightState;
    private readonly GeocodingService _geocoding;

    public MissionController(FlightStateService flightState, GeocodingService geocoding)
    {
        _flightState = flightState;
        _geocoding = geocoding;
    }

    [HttpPost("target")]
    public async Task<IActionResult> SetTarget([FromBody] TargetRequest request)
    {
        if (string.IsNullOrEmpty(request.Location))
            return BadRequest("Location is required.");

        // Geocode locally or accept coordinates? 
        // Let's let the .NET side handle geocoding to keep the "Tool Logic" close to the simulation, 
        // OR the Node MCP can do it. The user plan said "MCP Server... accept mission request".
        // Let's have the MCP server send the command "Navigate to X" and the .NET side handles execution.
        
        var coords = await _geocoding.GetCoordinatesAsync(request.Location);
        if (coords == null)
            return NotFound($"Location '{request.Location}' not found.");

        _flightState.SetNewDestination(coords.Value.Lat, coords.Value.Lng);
        return Ok(new { Message = $"Mission updated to {request.Location}", Lat = coords.Value.Lat, Lng = coords.Value.Lng });
    }
}

public class TargetRequest
{
    public string Location { get; set; } = string.Empty;
}
