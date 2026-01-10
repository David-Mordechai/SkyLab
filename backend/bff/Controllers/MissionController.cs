using Microsoft.AspNetCore.Mvc;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Controllers;

[ApiController]
[Route("api/mission")]
public class MissionController : ControllerBase
{
    private readonly FlightStateService _flightState;

    public MissionController(FlightStateService flightState)
    {
        _flightState = flightState;
    }

    [HttpPost("target")]
    public IActionResult SetTarget([FromBody] TargetRequest request)
    {
        if (request.Lat == 0 && request.Lng == 0)
            return BadRequest("Valid Lat/Lng coordinates are required.");
        
        _flightState.SetNewDestination(request.Lat, request.Lng);
        return Ok(new { Message = $"Mission updated to {request.Lat}, {request.Lng}", Lat = request.Lat, Lng = request.Lng });
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

public class TargetRequest 
{ 
    public double Lat { get; set; }
    public double Lng { get; set; }
}
public class SpeedRequest { public double Speed { get; set; } }
public class AltitudeRequest { public double Altitude { get; set; } }