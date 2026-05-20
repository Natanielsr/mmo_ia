using GameServerApp.World;

namespace GameServerApp.Contracts.Managers
{
    public interface IEquipmentManager
    {
        PlayerEquipment GetOrCreate(long playerId);
        PlayerEquipment? GetEquipment(long playerId);
        void Remove(long playerId);
    }
}
