using Microsoft.AspNetCore.SignalR;
using SkyLab.Backend.Hubs;

namespace SkyLab.Backend.Workers;

public class FlightSimulationWorker : BackgroundService
{
    private readonly IHubContext<FlightHub> _hubContext;
    private readonly ILogger<FlightSimulationWorker> _logger;
    private const double CenterLat = 31.801447; // Ashdod Latitude
    private const double CenterLng = 34.643497; // Ashdod Longitude
    private const double Radius = 0.01; // Approx 1km radius
    private double _angle = 0;
    private double _altitude = 4000; // Starting altitude in feet

    public FlightSimulationWorker(IHubContext<FlightHub> hubContext, ILogger<FlightSimulationWorker> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Simulation Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Calculate new position (Circular motion)
            // Updated to 20Hz (50ms) for smoother movement
            // Angle increment reduced to maintain same speed (0.05 / 20 = 0.0025)
            _angle += 0.0025;
            if (_angle > 360) _angle = 0;

            var lat = CenterLat + (Radius * Math.Cos(_angle));
            var lng = CenterLng + (Radius * Math.Sin(_angle));

            // Simulate slight altitude variation (sine wave) around 4000 feet
            _altitude = 4000 + (100 * Math.Sin(_angle * 2)); 

            // Simulate Speed (approx 100-110 knots with variation)
            var speed = 105 + (5 * Math.Cos(_angle * 3));

            // Calculate Heading
            var dLat = -Math.Sin(_angle);
            var dLng = Math.Cos(_angle);
            var headingRad = Math.Atan2(dLng, dLat);
            var headingDeg = headingRad * (180 / Math.PI);

            var flightId = "UAV-Ashdod-01";

            // Send altitude and speed
            await _hubContext.Clients.All.SendAsync("ReceiveFlightData", flightId, lat, lng, headingDeg, _altitude, speed, cancellationToken: stoppingToken);
            
            // Log less frequently to avoid spamming
            if (_angle % 0.5 < 0.003) 
            {
                 _logger.LogDebug($"Updated Flight {flightId}: {lat}, {lng}, {headingDeg}, {_altitude}, {speed}");
            }

            await Task.Delay(50, stoppingToken);
        }
    }
}
