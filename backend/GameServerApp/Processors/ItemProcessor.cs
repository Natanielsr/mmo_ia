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
            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return;

            bool used = inv.UseItem(itemId, player);
            if (!used) return;

            _worldEvents.OnPlayerHpChanged(new PlayerHpData
            {
                Id     = player.Id.ToString(),
                Hp     = player.Hp,
                MaxHp  = player.MaxHp,
                IsDead = false
            });
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
        }

        public void ProcessDropItem(IPlayer player, string itemId, Position targetPos)
        {
            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return;

            var item = inv.GetItems().FirstOrDefault(i => i.Id == itemId);
            if (item == null) return;

            IItem worldItem;
            if (item.Type == ItemType.Potion && item.Quantity > 1)
            {
                item.Quantity--;
                worldItem = new HealingPotion(
                    Guid.NewGuid().ToString(), item.Name, item.Weight, targetPos,
                    healAmount: (item as HealingPotion)?.HealAmount ?? 20,
                    description: item.Description, value: item.Value, tagName: item.TagName);
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
