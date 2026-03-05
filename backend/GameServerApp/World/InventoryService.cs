using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class InventoryService : IInventoryService
    {
        private readonly List<IItem> _items = new();

        public bool AddItem(IItem item)
        {
            if (item == null) return false;
            _items.Add(item);
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;
            return _items.Remove(item);
        }

        public bool UseItem(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;
            return true;
        }

        public bool DropItem(string itemId, Position dropPosition)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            item.Position = dropPosition;
            return _items.Remove(item);
        }

        public IList<IItem> GetItems()
        {
            return _items.AsReadOnly();
        }
    }
}
