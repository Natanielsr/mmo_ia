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
            
            // 1. Notify caller they joined
            await Clients.Caller.SendAsync("Joined", new { Name = player.Name, Position = player.Position });

            // 2. Send all existing players to the new player
            var otherPlayers = _sessions.Values
                .Where(p => p.Name != player.Name)
                .Select(p => new { PlayerId = p.Name, X = p.Position.X, Y = p.Position.Y });
            
            await Clients.Caller.SendAsync("SyncPlayers", otherPlayers);

            // 3. Notify everyone else that a new player joined
            await Clients.Others.SendAsync("PlayerJoined", new { PlayerId = player.Name, X = player.Position.X, Y = player.Position.Y });
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
