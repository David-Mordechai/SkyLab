namespace SkyLab.Backend.Services;

public enum FlightMode
{
    Orbiting,
    Transiting
}

public class FlightStateService
{
    // Current UAV Position
    public double CurrentLat { get; set; } = 31.801447;
    public double CurrentLng { get; set; } = 34.643497;
    
    // Target Orbit Center
    public double TargetLat { get; set; } = 31.801447;
    public double TargetLng { get; set; } = 34.643497;
    
    public FlightMode Mode { get; set; } = FlightMode.Orbiting;
    
    public double OrbitRadius { get; set; } = 0.01; // ~1km
    public double OrbitAngle { get; set; } = 0;

    public void SetNewDestination(double lat, double lng)
    {
        TargetLat = lat;
        TargetLng = lng;
        Mode = FlightMode.Transiting;
    }
}
