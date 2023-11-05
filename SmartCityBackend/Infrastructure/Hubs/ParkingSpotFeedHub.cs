using Microsoft.AspNetCore.SignalR;

namespace SmartCityBackend.Infrastructure.Hubs;

public interface ILocationClient
{
    Task ReceiveMessage(string message);
}

public class ParkingSpotFeedHub : Hub<ILocationClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId)
            .ReceiveMessage($"Connected to {nameof(ParkingSpotFeedHub)} with id '{Context.ConnectionId}'.");
    }
}