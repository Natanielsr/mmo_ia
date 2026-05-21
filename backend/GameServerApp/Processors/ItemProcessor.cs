using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

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
            var item = _itemManager.GetItemAt(player.Position);
            if (item == null) return;

            bool added = _inventoryManager.AddItem(player.Id, item);
            if (!added) return;

            _itemManager.RemoveItem(item.Id);
            _worldEvents.OnItemPickedUp(item.Id, player.Id);

            var inv = _inventoryManager.GetInventory(player.Id);
            _worldEvents.OnInventoryUpdated(player.Id, inv?.GetSlottedItems() ?? []);
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

            inv.DropItem(itemId, targetPos);
            _itemManager.DropItem(item);
            _worldEvents.OnItemDropped(item);
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
        }
    }
}
