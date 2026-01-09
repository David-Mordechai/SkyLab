using Microsoft.AspNetCore.SignalR;
using SkyLab.Backend.Hubs;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Workers;

public class FlightSimulationWorker : BackgroundService
{
    private readonly IHubContext<FlightHub> _hubContext;
    private readonly ILogger<FlightSimulationWorker> _logger;
    private readonly FlightStateService _state;
    
    // Physics Constants
    private const double SpeedDegreesPerTick = 0.000025; // Matches ~105 knots at 20Hz

    public FlightSimulationWorker(IHubContext<FlightHub> hubContext, ILogger<FlightSimulationWorker> logger, FlightStateService state)
    {
        _hubContext = hubContext;
        _logger = logger;
        _state = state;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Simulation Worker started with Physics Engine V2.");

        while (!stoppingToken.IsCancellationRequested)
        {
            double headingDeg = 0;
            double speedKts = 105;

            if (_state.Mode == FlightMode.Transiting)
            {
                // Vector to target
                double dLat = _state.TargetLat - _state.CurrentLat;
                double dLng = _state.TargetLng - _state.CurrentLng;
                double distance = Math.Sqrt(dLat * dLat + dLng * dLng);

                if (distance < _state.OrbitRadius)
                {
                    // Arrived! Switch to Orbit
                    _state.Mode = FlightMode.Orbiting;
                    // Calculate entry angle to avoid jump
                    _state.OrbitAngle = Math.Atan2(_state.CurrentLng - _state.TargetLng, _state.CurrentLat - _state.TargetLat);
                    _logger.LogInformation("Arrived at target. Switching to Orbit.");
                }
                else
                {
                    // Move towards target
                    double ratio = SpeedDegreesPerTick / distance;
                    _state.CurrentLat += dLat * ratio;
                    _state.CurrentLng += dLng * ratio;

                    // Calculate Heading
                    headingDeg = Math.Atan2(dLng, dLat) * (180 / Math.PI);
                }
            }
            
            if (_state.Mode == FlightMode.Orbiting)
            {
                // Circular motion
                _state.OrbitAngle += 0.0025; // Angular velocity
                if (_state.OrbitAngle > Math.PI * 2) _state.OrbitAngle -= Math.PI * 2;

                // Update position based on orbit center
                double prevLat = _state.CurrentLat;
                double prevLng = _state.CurrentLng;

                _state.CurrentLat = _state.TargetLat + (_state.OrbitRadius * Math.Cos(_state.OrbitAngle));
                _state.CurrentLng = _state.TargetLng + (_state.OrbitRadius * Math.Sin(_state.OrbitAngle));

                // Calculate Heading based on movement
                double dLat = _state.CurrentLat - prevLat;
                double dLng = _state.CurrentLng - prevLng;
                headingDeg = Math.Atan2(dLng, dLat) * (180 / Math.PI);
            }

            // Simulate slight altitude variation
            double altitude = 4000 + (100 * Math.Sin(DateTime.Now.Ticks / 10000000.0)); 
            
            // Add slight speed variation
            speedKts += (5 * Math.Cos(DateTime.Now.Ticks / 5000000.0));

            var flightId = "UAV-Ashdod-01";

            await _hubContext.Clients.All.SendAsync("ReceiveFlightData", flightId, _state.CurrentLat, _state.CurrentLng, headingDeg, altitude, speedKts, cancellationToken: stoppingToken);

            await Task.Delay(50, stoppingToken);
        }
    }
}