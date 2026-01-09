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
        private const double BaseStepPerKnotTick = 0.000025 / 105.0; // Normalized step per knot
    
        public FlightSimulationWorker(IHubContext<FlightHub> hubContext, ILogger<FlightSimulationWorker> logger, FlightStateService state)
        {
            _hubContext = hubContext;
            _logger = logger;
            _state = state;
        }
    
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Flight Simulation Worker started with Physics Engine V3 (Dynamic Telemetry).");
    
            while (!stoppingToken.IsCancellationRequested)
            {
                double headingDeg = 0;
    
                // Smoothly adjust Speed towards Target (Rate: 2 kts/sec)
                double speedDelta = _state.TargetSpeedKts - _state.CurrentSpeedKts;
                if (Math.Abs(speedDelta) > 0.1)
                    _state.CurrentSpeedKts += Math.Sign(speedDelta) * (2.0 / 20.0); // 20Hz update
    
                // Smoothly adjust Altitude towards Target (Rate: 10 ft/sec)
                double altDelta = _state.TargetAltitudeFt - _state.CurrentAltitudeFt;
                if (Math.Abs(altDelta) > 1.0)
                    _state.CurrentAltitudeFt += Math.Sign(altDelta) * (10.0 / 20.0);
    
                // Calculate movement step based on current speed
                double currentStep = _state.CurrentSpeedKts * BaseStepPerKnotTick;
    
                if (_state.Mode == FlightMode.Transiting)
                {
                    // Vector to target
                    double dLat = _state.TargetLat - _state.CurrentLat;
                    double dLng = _state.TargetLng - _state.CurrentLng;
                    double distance = Math.Sqrt(dLat * dLat + dLng * dLng);
    
                    if (distance < _state.OrbitRadius)
                    {
                        _state.Mode = FlightMode.Orbiting;
                        _state.OrbitAngle = Math.Atan2(_state.CurrentLng - _state.TargetLng, _state.CurrentLat - _state.TargetLat);
                        _logger.LogInformation("Arrived at target. Switching to Orbit.");
                    }
                    else
                    {
                        double ratio = currentStep / distance;
                        _state.CurrentLat += dLat * ratio;
                        _state.CurrentLng += dLng * ratio;
                        headingDeg = Math.Atan2(dLng, dLat) * (180 / Math.PI);
                    }
                }
                
                if (_state.Mode == FlightMode.Orbiting)
                {
                    // Angular velocity depends on linear speed: v = r * omega => omega = v / r
                    // currentStep is linear distance per tick. Angle step = currentStep / OrbitRadius
                    double angleStep = currentStep / _state.OrbitRadius;
                    _state.OrbitAngle += angleStep;
                    if (_state.OrbitAngle > Math.PI * 2) _state.OrbitAngle -= Math.PI * 2;
    
                    double prevLat = _state.CurrentLat;
                    double prevLng = _state.CurrentLng;
    
                    _state.CurrentLat = _state.TargetLat + (_state.OrbitRadius * Math.Cos(_state.OrbitAngle));
                    _state.CurrentLng = _state.TargetLng + (_state.OrbitRadius * Math.Sin(_state.OrbitAngle));
    
                    headingDeg = Math.Atan2(_state.CurrentLng - prevLng, _state.CurrentLat - prevLat) * (180 / Math.PI);
                }
    
                // Small oscillation for realism
                double displayAlt = _state.CurrentAltitudeFt + (5 * Math.Sin(DateTime.Now.Ticks / 10000000.0));
                double displaySpeed = _state.CurrentSpeedKts + (0.5 * Math.Cos(DateTime.Now.Ticks / 5000000.0));
    
                var flightId = "UAV-Ashdod-01";
    
                await _hubContext.Clients.All.SendAsync("ReceiveFlightData", flightId, _state.CurrentLat, _state.CurrentLng, headingDeg, displayAlt, displaySpeed, cancellationToken: stoppingToken);
    
                await Task.Delay(50, stoppingToken);
            }
        }
    }
    