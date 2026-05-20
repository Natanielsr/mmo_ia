using System.Collections.Concurrent;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServerApp.Managers
{
    public class InventoryManager : IInventoryManager
    {
        private readonly ConcurrentDictionary<long, PlayerInventory> _inventories = new();

        public PlayerInventory GetOrCreate(long playerId)
            => _inventories.GetOrAdd(playerId, _ => new PlayerInventory());

        public PlayerInventory? GetInventory(long playerId)
            => _inventories.TryGetValue(playerId, out var inv) ? inv : null;

        public void Remove(long playerId)
            => _inventories.TryRemove(playerId, out _);

        public bool AddItem(long playerId, IItem item)
            => GetOrCreate(playerId).AddItem(item);
    }
}
