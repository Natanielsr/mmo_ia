using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class PlayerInventory(int maxSlots = 20)
    {
        private readonly List<IItem> _items = new();
        private readonly int _maxSlots = maxSlots;

        public bool AddItem(IItem item)
        {
            if (item == null) return false;
            if (_items.Count >= _maxSlots) return false;
            _items.Add(item);
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;
            _items.Remove(item);
            return true;
        }

        public bool UseItem(string itemId, IPlayer player)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            if (item.Type != ItemType.Potion) return false;

            player.Heal(20);
            _items.Remove(item);
            return true;
        }

        public bool DropItem(string itemId, Position targetPos)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            item.Position = targetPos;
            _items.Remove(item);
            return true;
        }

        public IReadOnlyList<IItem> GetItems() => _items.AsReadOnly();
    }
}
