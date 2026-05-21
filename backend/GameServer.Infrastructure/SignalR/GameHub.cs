using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
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
        private readonly IItemManager _itemManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IEquipmentManager _equipmentManager;
        private readonly IHostEnvironment _env;

        public GameHub(
            IWorldManager worldProcessor,
            IWorldEvents worldEvents,
            ICollisionManager collisionManager,
            IIdGeneratorService idGeneratorService,
            IMonsterManager monsterManager,
            IPlayerManager playerManager,
            IItemManager itemManager,
            IInventoryManager inventoryManager,
            IEquipmentManager equipmentManager,
            IHostEnvironment env)
        {
            _worldProcessor = worldProcessor;
            _worldEvents = worldEvents;
            _collisionManager = collisionManager;
            _idGeneratorService = idGeneratorService;
            _monsterManager = monsterManager;
            _playerManager = playerManager;
            _itemManager = itemManager;
            _inventoryManager = inventoryManager;
            _equipmentManager = equipmentManager;
            _env = env;
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
            PlayerStatusData playerStatusData = new() { Id = player.Id.ToString(), Hp = player.Hp, MaxHp = player.MaxHp, IsDead = player.IsDead, Level = player.Level, Experience = player.Experience };

            // 1. Notify caller they joined
            await Clients.Caller.SendAsync("Joined", playerPositionData);
            await Clients.Caller.SendAsync("PlayerStatusUpdated", playerStatusData);

            // 2. Notify world events (which will notify other players)
            _worldEvents.OnPlayerJoined(playerPositionData);
            _worldEvents.OnPlayerStatusUpdated(playerStatusData);

            // 2.5 Load initial chunks
            _worldProcessor.ProcessChunkLoading(player, Context.ConnectionId);

            // 3. Send all existing players to the new player
            var otherPlayers = _playerManager.GetAllPlayers()
                .Where(p => p.Id != player.Id)
                .Select(p => new PlayerPositionData { Id = p.Id.ToString(), Name = p.Name, Position = p.Position });

            await Clients.Caller.SendAsync("SyncPlayers", otherPlayers);

            var otherPlayerStatuses = _playerManager.GetAllPlayers()
                .Where(p => p.Id != player.Id)
                .Select(p => new PlayerStatusData { Id = p.Id.ToString(), Hp = p.Hp, MaxHp = p.MaxHp, IsDead = p.IsDead, Level = p.Level, Experience = p.Experience });

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

            // 5. Send all dropped items to the new player
            var allItems = _itemManager.GetAllItems();
            var itemDataList = allItems.Select(item => new
            {
                Id = item.Id,
                Name = item.Name,
                Position = item.Position,
                Type = item.Type.ToString()
            }).ToList();

            await Clients.Caller.SendAsync("SyncItems", itemDataList);
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

        public void RequestUseItem(string itemId)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
                _worldProcessor.ProcessUseItem(player, itemId);
        }

        public void RequestDropItem(string itemId, int x, int y)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
                _worldProcessor.ProcessDropItem(player, itemId, new Position(x, y));
        }

        public void RequestEquipItem(string itemId)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
                _worldProcessor.ProcessEquipItem(player, itemId);
        }

        public void RequestUnequipItem(string slot)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null && System.Enum.TryParse<EquipmentSlot>(slot, ignoreCase: true, out var equipSlot))
                _worldProcessor.ProcessUnequipItem(player, equipSlot);
        }

        public void RequestMoveItemInInventory(string itemId, int toIndex)
        {
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
                _worldProcessor.ProcessMoveItemInInventory(player, itemId, toIndex);
        }

        // ===== Developer Cheat Methods =====
        private bool IsDev => _env.IsDevelopment();

        public void DevSpawnItem(string itemType)
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            IItem item = itemType.ToLower() switch
            {
                "weapon" => new Weapon(_idGeneratorService.GenerateId().ToString(), "Dev Sword", 1.0f, player.Position, 15),
                "helmet" => new Helmet(_idGeneratorService.GenerateId().ToString(), "Dev Helmet", 1.0f, player.Position, 10),
                "chest" => new Armor(_idGeneratorService.GenerateId().ToString(), "Dev Chest", 2.0f, player.Position, 10, EquipmentSlot.Chest),
                "legs" => new Legs(_idGeneratorService.GenerateId().ToString(), "Dev Legs", 1.5f, player.Position, 8),
                "boots" => new Boots(_idGeneratorService.GenerateId().ToString(), "Dev Boots", 0.5f, player.Position, 5),
                "shield" => new Shield(_idGeneratorService.GenerateId().ToString(), "Dev Shield", 2.0f, player.Position, 12),
                "potion" => new HealingPotion(_idGeneratorService.GenerateId().ToString(), player.Position),
                _ => new Weapon(_idGeneratorService.GenerateId().ToString(), "Dev Item", 1.0f, player.Position, 10)
            };

            _inventoryManager.AddItem(player.Id, item);
            var inventory = _inventoryManager.GetInventory(player.Id);
            if (inventory != null)
                _worldEvents.OnInventoryUpdated(player.Id, inventory.GetSlottedItems());
        }

        public void DevSetLevel(int level)
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            player.SetLevel(level);
            _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
            {
                Id = player.Id.ToString(),
                Level = player.Level,
                Experience = player.Experience,
                Hp = player.Hp,
                MaxHp = player.MaxHp
            });
        }

        public void DevGodMode(bool enabled)
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            player.SetGodMode(enabled);
        }

        public void DevFullHeal()
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            player.Heal(player.MaxHp);
            _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
            {
                Id = player.Id.ToString(),
                Level = player.Level,
                Experience = player.Experience,
                Hp = player.Hp,
                MaxHp = player.MaxHp
            });
        }

        public void DevOneHitKill()
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            var allMonsters = _monsterManager.GetAllMonsters();
            var monstersNearby = allMonsters.Where(m =>
                Math.Abs(m.Position.X - player.Position.X) <= 5 &&
                Math.Abs(m.Position.Y - player.Position.Y) <= 5
            ).ToList();

            foreach (var monster in monstersNearby)
            {
                monster.TakeDamage(int.MaxValue);
                if (monster.IsDead)
                {
                    _worldEvents.OnMonsterDied(monster.Id.ToString());
                    _monsterManager.RemoveMonster(monster.Id);
                }
            }
        }

        public void DevGiveAllItems()
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            var items = new IItem[]
            {
                new Weapon(_idGeneratorService.GenerateId().ToString(), "Dev Sword", 1.0f, player.Position, 15),
                new Helmet(_idGeneratorService.GenerateId().ToString(), "Dev Helmet", 1.0f, player.Position, 10),
                new Armor(_idGeneratorService.GenerateId().ToString(), "Dev Chest", 2.0f, player.Position, 10, EquipmentSlot.Chest),
                new Legs(_idGeneratorService.GenerateId().ToString(), "Dev Legs", 1.5f, player.Position, 8),
                new Boots(_idGeneratorService.GenerateId().ToString(), "Dev Boots", 0.5f, player.Position, 5),
                new Shield(_idGeneratorService.GenerateId().ToString(), "Dev Shield", 2.0f, player.Position, 12),
                new HealingPotion(_idGeneratorService.GenerateId().ToString(), player.Position)
            };

            foreach (var item in items)
                _inventoryManager.AddItem(player.Id, item);

            var inventory = _inventoryManager.GetInventory(player.Id);
            if (inventory != null)
                _worldEvents.OnInventoryUpdated(player.Id, inventory.GetSlottedItems());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_playerManager.RemovePlayer(Context.ConnectionId, out var player) && player != null)
            {
                _worldEvents.OnPlayerLeft(player.Id);
                _collisionManager.RemoveObject(player.Id);
                _inventoryManager.Remove(player.Id);
                _equipmentManager.Remove(player.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
