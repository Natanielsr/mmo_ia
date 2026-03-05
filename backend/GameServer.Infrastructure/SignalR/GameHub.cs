using Microsoft.AspNetCore.SignalR;

namespace GameServer.Infrastructure.SignalR
{
    public class GameHub : Hub
    {
        // Hub methods to be called from the client (e.g., SendMessage, RequestMove)
        // For now, it serves as the real-time pipe.
        
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // Logic for when a player connects (e.g., authentication check)
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
