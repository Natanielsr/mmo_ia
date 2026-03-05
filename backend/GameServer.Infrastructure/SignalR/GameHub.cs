using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.World;
using System.Collections.Concurrent;

namespace GameServer.Infrastructure.SignalR
{
    public class GameHub : Hub
    {
        private readonly IWorldProcessor _worldProcessor;
        private static readonly ConcurrentDictionary<string, IPlayer> _sessions = new();

        public GameHub(IWorldProcessor worldProcessor)
        {
            _worldProcessor = worldProcessor;
        }

        public async Task JoinGame(string playerName)
        {
            // Simple logic: create or get player
            var player = _sessions.GetOrAdd(Context.ConnectionId, _ => new Player(playerName, new Position(0, 0)));
            await Clients.Caller.SendAsync("Joined", new { Name = player.Name, Position = player.Position });
        }

        public async Task RequestMove(string direction)
        {
            if (_sessions.TryGetValue(Context.ConnectionId, out var player))
            {
                bool success = _worldProcessor.ProcessPlayerMovement(player, direction);
                if (!success)
                {
                    await Clients.Caller.SendAsync("MoveFailed", "Caminho bloqueado ou ação inválida.");
                }
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _sessions.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
