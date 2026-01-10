using Microsoft.AspNetCore.SignalR;
using SkyLab.Backend.Hubs;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Workers;

public class FlightSimulationWorker : BackgroundService
{
    private readonly IHubContext<FlightHub> _hubContext;
    private readonly ILogger<FlightSimulationWorker> _logger;
    private readonly FlightStateService _state;

    public FlightSimulationWorker(IHubContext<FlightHub> hubContext, ILogger<FlightSimulationWorker> logger, FlightStateService state)
    {
        _hubContext = hubContext;
        _logger = logger;
        _state = state;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Simulation Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Update Physics
            _state.UpdatePhysics();

            // 2. Add some visual noise/interpolation for display
            double displayAlt = _state.CurrentAltitudeFt + (5 * Math.Sin(DateTime.Now.Ticks / 10000000.0));
            double displaySpeed = _state.CurrentSpeedKts + (0.5 * Math.Cos(DateTime.Now.Ticks / 5000000.0));
            double heading = _state.GetHeading();

            // 3. Broadcast State
            var flightId = "UAV-Ashdod-01";
            await _hubContext.Clients.All.SendAsync(
                "ReceiveFlightData", 
                flightId, 
                _state.CurrentLat, 
                _state.CurrentLng, 
                heading, 
                displayAlt, 
                displaySpeed, 
                _state.TargetLat, 
                _state.TargetLng, 
                cancellationToken: stoppingToken
            );

            await Task.Delay(50, stoppingToken);
        }
    }
}