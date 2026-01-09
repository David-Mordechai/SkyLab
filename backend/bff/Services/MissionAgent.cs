namespace SkyLab.Backend.Services;

public class MissionAgent
{
    private readonly FlightStateService _flightState;
    private readonly GeocodingService _geocoding;
    private readonly ILogger<MissionAgent> _logger;

    public MissionAgent(FlightStateService flightState, GeocodingService geocoding, ILogger<MissionAgent> logger)
    {
        _flightState = flightState;
        _geocoding = geocoding;
        _logger = logger;
    }

    public async Task<string> ProcessCommandAsync(string command)
    {
        _logger.LogInformation($"Processing command: {command}");

        // Simple heuristic for demo: "fly to [location]" or "go to [location]"
        var lowerCommand = command.ToLower();
        if (lowerCommand.Contains("fly to") || lowerCommand.Contains("go to") || lowerCommand.Contains("over"))
        {
            var location = ExtractLocation(lowerCommand);
            if (string.IsNullOrEmpty(location))
                return "I couldn't identify the location in your command.";

            var coords = await _geocoding.GetCoordinatesAsync(location);
            if (coords == null)
                return $"I found no coordinates for '{location}'.";

            _flightState.SetNewDestination(coords.Value.Lat, coords.Value.Lng);
            return $"Mission updated: Taking course to {location} ({coords.Value.Lat:F4}, {coords.Value.Lng:F4}). Switching to TRANSIT mode.";
        }

        return "I didn't understand that command. Try 'Fly over Tel Aviv'.";
    }

    private string ExtractLocation(string command)
    {
        string[] prefixes = { "fly to ", "go to ", "fly over ", "over " };
        foreach (var prefix in prefixes)
        {
            int index = command.IndexOf(prefix);
            if (index != -1)
            {
                return command.Substring(index + prefix.Length).Trim();
            }
        }
        return string.Empty;
    }
}
