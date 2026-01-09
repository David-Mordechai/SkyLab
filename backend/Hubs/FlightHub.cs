using Microsoft.AspNetCore.SignalR;

namespace SkyLab.Backend.Hubs;

public class FlightHub : Hub
{
    public async Task SendFlightData(string flightId, double latitude, double longitude)
    {
        await Clients.All.SendAsync("ReceiveFlightData", flightId, latitude, longitude);
    }
}
