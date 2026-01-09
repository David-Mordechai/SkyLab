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
        
        var coords = await _geocoding.GetCoordinatesAsync(request.Location);
        if (coords == null)
            return NotFound($"Location '{request.Location}' not found.");

        _flightState.SetNewDestination(coords.Value.Lat, coords.Value.Lng);
        return Ok(new { Message = $"Mission updated to {request.Location}", Lat = coords.Value.Lat, Lng = coords.Value.Lng });
    }

    [HttpPost("speed")]
    public IActionResult SetSpeed([FromBody] SpeedRequest request)
    {
        if (request.Speed <= 0 || request.Speed > 500)
            return BadRequest("Invalid speed range (1-500 kts).");

        _flightState.SetSpeed(request.Speed);
        return Ok(new { Message = $"Target speed set to {request.Speed} kts" });
    }

    [HttpPost("altitude")]
    public IActionResult SetAltitude([FromBody] AltitudeRequest request)
    {
        if (request.Altitude < 0 || request.Altitude > 60000)
            return BadRequest("Invalid altitude range (0-60000 ft).");

        _flightState.SetAltitude(request.Altitude);
        return Ok(new { Message = $"Target altitude set to {request.Altitude} ft" });
    }
}

public class TargetRequest { public string Location { get; set; } = string.Empty; }
public class SpeedRequest { public double Speed { get; set; } }
public class AltitudeRequest { public double Altitude { get; set; } }
