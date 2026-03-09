using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.World;
using System.Collections.Concurrent;
using GameServerApp.Dtos;
using GameServer.Infrastructure.Services;
using GameServerApp.Contracts.Services;

namespace GameServer.Infrastructure.SignalR
{
    public class GameHub : Hub
    {
        private readonly IWorldProcessor _worldProcessor;
        private readonly IWorldEvents _worldEvents;
        private readonly ICollisionManager _collisionManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private static readonly ConcurrentDictionary<string, IPlayer> _sessions = new();

        public GameHub(
            IWorldProcessor worldProcessor,
            IWorldEvents worldEvents,
            ICollisionManager collisionManager,
            IIdGeneratorService idGeneratorService
            )
        {
            _worldProcessor = worldProcessor;
            _worldEvents = worldEvents;
            _collisionManager = collisionManager;
            _idGeneratorService = idGeneratorService;
        }

        public async Task JoinGame(string playerName)
        {

            // Simple logic: create or get player
            var player = _sessions.GetOrAdd(Context.ConnectionId,
            _ => new Player(_idGeneratorService.GenerateId(), playerName, new Position(0, 0)));

            // Register player collision
            _collisionManager.RegisterDynamicObject(player);

            PlayerPositionData playerPositionData = new() { Id = player.Id, Name = player.Name, Position = player.Position };

            // 1. Notify caller they joined
            await Clients.Caller.SendAsync("Joined", playerPositionData);

            // 2. Notify world events (which will notify other players)
            _worldEvents.OnPlayerJoined(playerPositionData);

            // 3. Send all existing players to the new player
            var otherPlayers = _sessions.Values
                .Where(p => p.Id != player.Id)
                .Select(p => new PlayerPositionData { Id = p.Id, Name = p.Name, Position = p.Position });

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
                _worldEvents.OnPlayerLeft(player.Id);
                _collisionManager.RemoveObject(player.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
