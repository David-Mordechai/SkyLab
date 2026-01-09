using Microsoft.AspNetCore.SignalR;

namespace SkyLab.Backend.Hubs;

public class FlightHub : Hub
{
    public async Task SendFlightData(string flightId, double latitude, double longitude, double heading, double altitude, double speed)
    {
        await Clients.All.SendAsync("ReceiveFlightData", flightId, latitude, longitude, heading, altitude, speed);
    }
}
