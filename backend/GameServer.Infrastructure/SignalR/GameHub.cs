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
        private readonly IWorldEvents _worldEvents;
        private readonly ICollisionManager _collisionManager;
        private static readonly ConcurrentDictionary<string, IPlayer> _sessions = new();

        public GameHub(IWorldProcessor worldProcessor, IWorldEvents worldEvents, ICollisionManager collisionManager)
        {
            _worldProcessor = worldProcessor;
            _worldEvents = worldEvents;
            _collisionManager = collisionManager;
        }

        public async Task JoinGame(string playerName)
        {
            // Simple logic: create or get player
            var player = _sessions.GetOrAdd(Context.ConnectionId, _ => new Player(playerName, new Position(0, 0)));
            
            // Register player collision
            _collisionManager.RegisterObject(player);
            
            // 1. Notify caller they joined
            await Clients.Caller.SendAsync("Joined", new { Name = player.Name, Position = player.Position });

            // 2. Notify world events (which will notify other players)
            _worldEvents.OnPlayerJoined(player.Name, player.Position);

            // 3. Send all existing players to the new player
            var otherPlayers = _sessions.Values
                .Where(p => p.Name != player.Name)
                .Select(p => new { PlayerId = p.Name, X = p.Position.X, Y = p.Position.Y });
            
            await Clients.Caller.SendAsync("SyncPlayers", otherPlayers);
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_sessions.TryRemove(Context.ConnectionId, out var player))
            {
                _worldEvents.OnPlayerLeft(player.Name);
                _collisionManager.RemoveObject(player.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
