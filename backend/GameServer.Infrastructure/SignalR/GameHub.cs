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
        private readonly IWorldManager _worldProcessor;
        private readonly IWorldEvents _worldEvents;
        private readonly ICollisionManager _collisionManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly IMonsterManager _monsterManager;
        private readonly IPlayerManager _playerManager;

        public GameHub(
            IWorldManager worldProcessor,
            IWorldEvents worldEvents,
            ICollisionManager collisionManager,
            IIdGeneratorService idGeneratorService,
            IMonsterManager monsterManager,
            IPlayerManager playerManager
            )
        {
            _worldProcessor = worldProcessor;
            _worldEvents = worldEvents;
            _collisionManager = collisionManager;
            _idGeneratorService = idGeneratorService;
            _monsterManager = monsterManager;
            _playerManager = playerManager;
        }

        public async Task JoinGame(string playerName)
        {

            // Simple logic: create or get player
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null)
            {
                player = new Player(_idGeneratorService.GenerateId(), playerName, new Position(0, 0));
                _playerManager.AddPlayer(Context.ConnectionId, player);
            }

            // Register player collision
            _collisionManager.RegisterDynamicObject(player);

            PlayerPositionData playerPositionData = new() { Id = player.Id.ToString(), Name = player.Name, Position = player.Position };
            PlayerStatusData playerStatusData = new() { Id = player.Id.ToString(), Hp = player.Hp, MaxHp = player.MaxHp, IsDead = player.IsDead };

            // 1. Notify caller they joined
            await Clients.Caller.SendAsync("Joined", playerPositionData);
            await Clients.Caller.SendAsync("PlayerStatusUpdated", playerStatusData);

            // 2. Notify world events (which will notify other players)
            _worldEvents.OnPlayerJoined(playerPositionData);
            _worldEvents.OnPlayerStatusUpdated(playerStatusData);

            // 3. Send all existing players to the new player
            var otherPlayers = _playerManager.GetAllPlayers()
                .Where(p => p.Id != player.Id)
                .Select(p => new PlayerPositionData { Id = p.Id.ToString(), Name = p.Name, Position = p.Position });

            await Clients.Caller.SendAsync("SyncPlayers", otherPlayers);

            var otherPlayerStatuses = _playerManager.GetAllPlayers()
                .Where(p => p.Id != player.Id)
                .Select(p => new PlayerStatusData { Id = p.Id.ToString(), Hp = p.Hp, MaxHp = p.MaxHp, IsDead = p.IsDead });

            await Clients.Caller.SendAsync("SyncPlayerStatuses", otherPlayerStatuses);

            // 4. Send all monsters to the new player
            var allMonsters = _monsterManager.GetAllMonsters();
            var monsterDataList = allMonsters.Select(monster => new MonsterData
            {
                Id = monster.Id.ToString(),
                Name = monster.Name,
                ObjectCode = monster.ObjectCode,
                Position = monster.Position,
                Hp = monster.Hp,
                MaxHp = monster.MaxHp,
                AttackPower = monster.AttackPower,
                IsDead = monster.IsDead
            }).ToList();

            await Clients.Caller.SendAsync("SyncMonsters", monsterDataList);
        }

        public async Task RequestMove(string direction)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
            {
                bool success = _worldProcessor.ProcessPlayerMovement(player, direction);
                if (!success)
                {
                    await Clients.Caller.SendAsync("MoveFailed", "Caminho bloqueado ou ação inválida.");
                }
            }
        }

        public void RequestAttack(string targetId)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
            {
                _worldProcessor.ProcessPlayerAttackMonster(player, targetId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_playerManager.RemovePlayer(Context.ConnectionId, out var player) && player != null)
            {
                _worldEvents.OnPlayerLeft(player.Id);
                _collisionManager.RemoveObject(player.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
