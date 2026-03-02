namespace GameServerApp.Contracts
{
    using GameServerApp.Contracts.Types;

    public interface IInventoryService
    {
        bool AddItem(IItem item);
        bool RemoveItem(string itemId);
        bool UseItem(string itemId);
        bool DropItem(string itemId, Position dropPosition);
        IList<IItem> GetItems();
    }
}
