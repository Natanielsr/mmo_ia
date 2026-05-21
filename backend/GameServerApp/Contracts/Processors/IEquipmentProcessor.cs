using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Processors
{
    public interface IEquipmentProcessor
    {
        bool ProcessEquipItem(IPlayer player, string itemId);
        bool ProcessUnequipItem(IPlayer player, EquipmentSlot slot);
        bool ProcessMoveItemInInventory(IPlayer player, string itemId, int toIndex);
    }
}
