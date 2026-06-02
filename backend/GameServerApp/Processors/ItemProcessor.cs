using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;

namespace GameServerApp.Processors
{
    public class ItemProcessor : IItemProcessor
    {
        private readonly IItemManager _itemManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IWorldEvents _worldEvents;

        public ItemProcessor(
            IItemManager itemManager,
            IInventoryManager inventoryManager,
            IWorldEvents worldEvents)
        {
            _itemManager = itemManager;
            _inventoryManager = inventoryManager;
            _worldEvents = worldEvents;
        }

        public void ProcessItemPickup(IPlayer player)
        {
            if (player.State == PlayerState.Dead) return;
            var items = _itemManager.GetItemsAt(player.Position).ToList();
            if (items.Count == 0) return;

            bool anyPickedUp = false;
            foreach (var item in items)
            {
                bool added = _inventoryManager.AddItem(player.Id, item);
                if (!added) continue;

                _itemManager.RemoveItem(item.Id);
                _worldEvents.OnItemPickedUp(item.Id, player.Id);
                anyPickedUp = true;
            }

            if (anyPickedUp)
            {
                var inv = _inventoryManager.GetInventory(player.Id);
                _worldEvents.OnInventoryUpdated(player.Id, inv?.GetSlottedItems() ?? []);
            }
        }

        public void ProcessUseItem(IPlayer player, string itemId)
        {
            if (player.State == PlayerState.Dead) return;
            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return;

            var item = inv.GetItems().FirstOrDefault(i => i.Id == itemId);
            bool used = inv.UseItem(itemId, player);
            if (!used) return;

            var hpData = new PlayerHpData
            {
                Id     = player.Id.ToString(),
                Hp     = player.Hp,
                MaxHp  = player.MaxHp,
                IsDead = false
            };

            _worldEvents.OnPlayerHpChanged(hpData);
            _worldEvents.OnPlayerHealed(hpData);
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
        }

        public void ProcessDropItem(IPlayer player, string itemId, Position targetPos)
        {
            if (player.State == PlayerState.Dead) return;
            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return;

            var item = inv.GetItems().FirstOrDefault(i => i.Id == itemId);
            if (item == null) return;

            IItem worldItem;
            if (item is HealingItem hi && item.Quantity > 1)
            {
                item.Quantity--;
                worldItem = new HealingItem(
                    Guid.NewGuid().ToString(), item.Name, item.Weight, item.TagName, targetPos,
                    item.Type, hi.HealAmount, item.Description, item.Value);
            }
            else
            {
                inv.DropItem(itemId, targetPos);
                worldItem = item;
            }

            _itemManager.DropItem(worldItem);
            _worldEvents.OnItemDropped(worldItem);
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
        }
    }
}
