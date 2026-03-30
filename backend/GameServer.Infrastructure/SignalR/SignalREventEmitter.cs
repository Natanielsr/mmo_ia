using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServer.Infrastructure.SignalR
{
    public partial class SignalREventEmitter : IWorldEvents
    {
        private readonly IHubContext<GameHub> _hubContext;

        public SignalREventEmitter(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void OnPlayerMoved(PlayerPositionData playerPositionData)
        {
            _hubContext.Clients.All.SendAsync("PlayerMoved", playerPositionData);
        }

        public void OnPlayerStatusUpdated(PlayerStatusData playerStatusData)
        {
            _hubContext.Clients.All.SendAsync("PlayerStatusUpdated", playerStatusData);
        }

        public void OnPlayerJoined(PlayerPositionData playerPositionData)
        {
            _hubContext.Clients.All.SendAsync("PlayerJoined", playerPositionData);
        }

        public void OnPlayerAttacked(PlayerAttackData attackData)
        {
            _hubContext.Clients.All.SendAsync("PlayerAttacked", attackData);
        }

        public void OnPlayerDied(long playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerDied", playerId.ToString());
        }

        public void OnPlayerExperienceGained(long playerId, long amount, long totalExperience)
        {
            _hubContext.Clients.User(playerId.ToString()).SendAsync("ExperienceGained", new
            {
                Amount = amount,
                Total = totalExperience
            });
        }

        public void OnPlayerLevelUp(long playerId, int newLevel)
        {
            _hubContext.Clients.All.SendAsync("PlayerLevelUp", new
            {
                PlayerId = playerId.ToString(),
                Level = newLevel
            });
        }

        public void OnPlayerLeft(long playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerLeft", playerId.ToString());
        }

        // --- Eventos de Monstros ---
        public void OnMonsterSpawned(MonsterData monsterData)
        {
            _hubContext.Clients.All.SendAsync("MonsterSpawned", monsterData);
        }

        public void OnMonsterMoved(MonsterData monsterData)
        {
            _hubContext.Clients.All.SendAsync("MonsterMoved", monsterData);
        }

        public void OnMonsterDied(string monsterId)
        {
            _hubContext.Clients.All.SendAsync("MonsterDied", monsterId);
        }

        public void OnMonsterDamaged(string monsterId, int damage, int currentHp)
        {
            _hubContext.Clients.All.SendAsync("MonsterDamaged", new { Id = monsterId, Damage = damage, CurrentHp = currentHp });
        }

        public void OnItemDropped(IItem item)
        {
            _hubContext.Clients.All.SendAsync("ItemDropped", new
            {
                Id = item.Id,
                Name = item.Name,
                Position = item.Position,
                Type = item.Type.ToString()
            });
        }

        public void OnItemPickedUp(string itemId, long playerId)
        {
            _hubContext.Clients.All.SendAsync("ItemPickedUp", new
            {
                ItemId = itemId,
                PlayerId = playerId.ToString()
            });
        }

        public void OnChunkLoaded(string connectionId, ChunkData chunkData)
        {
            // Envia apenas para o jogador que está carregando o chunk
            _hubContext.Clients.Client(connectionId).SendAsync("ChunkLoaded", chunkData);
        }
    }
}
