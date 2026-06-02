using Microsoft.Extensions.Hosting;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;

namespace GameServer.Infrastructure.SignalR
{
    public partial class GameHub
    {
        private bool IsDev => _env.IsDevelopment();

        public void DevSpawnItem(string itemType)
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            IItem item = itemType.ToLower() switch
            {
                "weapon" => new Weapon(_idGeneratorService.GenerateId().ToString(), "Dagger", 1.5f, "dagger", player.Position, attackBonus: 5),
                "helmet" => new Helmet(_idGeneratorService.GenerateId().ToString(), "Iron Helmet", 1.2f, "iron-helmet", player.Position, defenseBonus: 1),
                "chest"  => new Armor(_idGeneratorService.GenerateId().ToString(), "Leather Vest", 2.0f, "leather-vest", player.Position, defenseBonus: 1, EquipmentSlot.Chest),
                "legs"   => new Legs(_idGeneratorService.GenerateId().ToString(), "Leather Pants", 1.0f, "leather-pants", player.Position, defenseBonus: 1),
                "boots"  => new Boots(_idGeneratorService.GenerateId().ToString(), "Iron Boots", 1.5f, "iron-boots", player.Position, defenseBonus: 1),
                "shield" => new Shield(_idGeneratorService.GenerateId().ToString(), "Wooden Shield", 2.5f, "wooden-shield", player.Position, defenseBonus: 1),
                "potion" => new HealingItem(_idGeneratorService.GenerateId().ToString(), "Healing Potion", 0.5f, "potion", player.Position, ItemType.Potion),
                _        => new Weapon(_idGeneratorService.GenerateId().ToString(), "Dagger", 1.5f, "dagger", player.Position, attackBonus: 5)
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

        public void DevSpawnItemByTag(string tagName)
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            var def = _itemRepo.GetByTagName(tagName);
            if (def == null)
            {
                return;
            }

            var item = _itemFactory.Create(def, _idGeneratorService.GenerateId().ToString(), player.Position);
            _itemManager.DropItem(item);
            _worldEvents.OnItemDropped(item);
        }

        public void DevGiveAllItems()
        {
            if (!IsDev) return;
            var player = _playerManager.GetPlayerByConnectionId(Context.ConnectionId);
            if (player == null) return;

            var items = new IItem[]
            {
                new Weapon(_idGeneratorService.GenerateId().ToString(), "Dagger", 1.5f, "dagger", player.Position, attackBonus: 5),
                new Helmet(_idGeneratorService.GenerateId().ToString(), "Iron Helmet", 1.2f, "iron-helmet", player.Position, defenseBonus: 1),
                new Armor(_idGeneratorService.GenerateId().ToString(), "Leather Vest", 2.0f, "leather-vest", player.Position, defenseBonus: 1, EquipmentSlot.Chest),
                new Legs(_idGeneratorService.GenerateId().ToString(), "Leather Pants", 1.0f, "leather-pants", player.Position, defenseBonus: 1),
                new Boots(_idGeneratorService.GenerateId().ToString(), "Iron Boots", 1.5f, "iron-boots", player.Position, defenseBonus: 1),
                new Shield(_idGeneratorService.GenerateId().ToString(), "Wooden Shield", 2.5f, "wooden-shield", player.Position, defenseBonus: 1),
                new HealingItem(_idGeneratorService.GenerateId().ToString(), "Healing Potion", 0.5f, "potion", player.Position, ItemType.Potion)
            };

            foreach (var item in items)
                _inventoryManager.AddItem(player.Id, item);

            var inventory = _inventoryManager.GetInventory(player.Id);
            if (inventory != null)
                _worldEvents.OnInventoryUpdated(player.Id, inventory.GetSlottedItems());
        }
    }
}
