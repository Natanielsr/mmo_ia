using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Managers
{
    public interface IItemManager
    {
        void DropItem(IItem item);
        IItem? GetItemAt(Position position);
        void RemoveItem(string itemId);
        IReadOnlyCollection<IItem> GetAllItems();
        IReadOnlyCollection<IItem> GetItemsInChunk(ChunkCoord coord);
    }
}
