using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using GameServerApp.World;
using System.Collections.Generic;

namespace GameServer.Infrastructure.SignalR
{
    public partial class SignalREventEmitter : IWorldEvents
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IPlayerManager _playerManager;

        public SignalREventEmitter(IHubContext<GameHub> hubContext, IPlayerManager playerManager)
        {
            _hubContext = hubContext;
            _playerManager = playerManager;
        }

        public void OnPlayerMoved(PlayerData playerPositionData)
        {
            _hubContext.Clients.All.SendAsync("PlayerMoved", playerPositionData);
        }

        public void OnPlayerHpChanged(PlayerHpData playerHpData)
        {
            _hubContext.Clients.All.SendAsync("PlayerHpChanged", playerHpData);
        }

        public void OnPlayerStatusUpdated(PlayerStatusData playerStatusData)
        {
            _hubContext.Clients.All.SendAsync("PlayerStatusUpdated", playerStatusData);
        }

        public void OnPlayerJoined(PlayerData playerPositionData)
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

        public void OnMonsterRemoved(string monsterId)
        {
            _hubContext.Clients.All.SendAsync("MonsterRemoved", monsterId);
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

        public void OnInventoryUpdated(long playerId, IReadOnlyList<(int SlotIndex, IItem Item)> slots)
        {
            var connId = _playerManager.GetConnectionIdByPlayerId(playerId);
            if (connId == null) return;

            var payload = slots.Select(s => new ItemData
            {
                Id           = s.Item.Id,
                Name         = s.Item.Name,
                Position     = s.Item.Position,
                Type         = s.Item.Type,
                AttackBonus  = (s.Item as Weapon)?.AttackBonus,
                DefenseBonus = (s.Item as Armor)?.DefenseBonus,
                SlotIndex    = s.SlotIndex,
                Quantity     = s.Item.Quantity,
            });

            _hubContext.Clients.Client(connId).SendAsync("InventoryUpdated", payload);
        }

        public void OnEquipmentUpdated(long playerId, IReadOnlyDictionary<EquipmentSlot, IItem?> slots)
        {
            var connId = _playerManager.GetConnectionIdByPlayerId(playerId);
            if (connId == null) return;

            var payload = slots.ToDictionary(
                kv => kv.Key.ToString(),
                kv => kv.Value == null ? null : (object?)new
                {
                    Id           = kv.Value.Id,
                    Name         = kv.Value.Name,
                    Type         = kv.Value.Type.ToString(),
                    AttackBonus  = (kv.Value as Weapon)?.AttackBonus,
                    DefenseBonus = (kv.Value as Armor)?.DefenseBonus,
                });

            _hubContext.Clients.Client(connId).SendAsync("EquipmentUpdated", payload);
        }

        public void OnChunkLoaded(string connectionId, ChunkData chunkData)
        {
            // Envia apenas para o jogador que está carregando o chunk
            _hubContext.Clients.Client(connectionId).SendAsync("ChunkLoaded", chunkData);
        }

        public void OnPlayerWeaponEquipped(string playerId, string? weaponName)
        {
            _hubContext.Clients.All.SendAsync("PlayerWeaponEquipped", new { PlayerId = playerId, WeaponName = weaponName });
        }

        public void OnPlayerArmorEquipped(string playerId, string? armorName)
        {
            _hubContext.Clients.All.SendAsync("PlayerArmorEquipped", new { PlayerId = playerId, ArmorName = armorName });
        }

        public void OnPlayerLegsEquipped(string playerId, string? legsName)
        {
            _hubContext.Clients.All.SendAsync("PlayerLegsEquipped", new { PlayerId = playerId, LegsName = legsName });
        }
    }
}
