using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using System.Collections.Concurrent;

namespace GameServerApp.Managers
{
    public class ItemManager : IItemManager
    {
        private readonly ConcurrentDictionary<string, IItem> _items = new();

        public void DropItem(IItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(item.Id)) throw new ArgumentException("Item ID cannot be null or empty", nameof(item));

            _items.TryAdd(item.Id, item);
        }

        public IItem? GetItemAt(Position position)
        {
            if (position == null) return null;
            return _items.Values.FirstOrDefault(i => i.Position.X == position.X && i.Position.Y == position.Y);
        }

        public void RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _items.TryRemove(itemId, out _);
        }

        public IReadOnlyCollection<IItem> GetAllItems()
        {
            return _items.Values.ToList();
        }
    }
}
