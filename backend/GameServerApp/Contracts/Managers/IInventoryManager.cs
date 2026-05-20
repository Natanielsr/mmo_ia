using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServerApp.Contracts.Managers
{
    public interface IInventoryManager
    {
        PlayerInventory GetOrCreate(long playerId);
        PlayerInventory? GetInventory(long playerId);
        void Remove(long playerId);
        bool AddItem(long playerId, IItem item);
    }
}
