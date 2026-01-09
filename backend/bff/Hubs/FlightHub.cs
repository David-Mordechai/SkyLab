using Microsoft.AspNetCore.SignalR;
using SkyLab.Backend.Services;

namespace SkyLab.Backend.Hubs;

public class FlightHub : Hub
{
    private readonly MissionAgent _missionAgent;

    public FlightHub(MissionAgent missionAgent)
    {
        _missionAgent = missionAgent;
    }

    public async Task SendFlightData(string flightId, double latitude, double longitude, double heading, double altitude, double speed)
    {
        await Clients.All.SendAsync("ReceiveFlightData", flightId, latitude, longitude, heading, altitude, speed);
    }

    public async Task ProcessChatMessage(string user, string message)
    {
        // Broadcast user message
        await Clients.All.SendAsync("ReceiveChatMessage", user, message);

        // Process with Mission Agent
        var response = await _missionAgent.ProcessCommandAsync(message);

        // Broadcast system response
        await Clients.All.SendAsync("ReceiveChatMessage", "Mission Control", response);
    }
}
