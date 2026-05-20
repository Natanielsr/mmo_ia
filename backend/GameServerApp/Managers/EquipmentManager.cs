using System.Collections.Concurrent;
using GameServerApp.Contracts.Managers;
using GameServerApp.World;

namespace GameServerApp.Managers
{
    public class EquipmentManager : IEquipmentManager
    {
        private readonly ConcurrentDictionary<long, PlayerEquipment> _equipments = new();

        public PlayerEquipment GetOrCreate(long playerId)
            => _equipments.GetOrAdd(playerId, _ => new PlayerEquipment());

        public PlayerEquipment? GetEquipment(long playerId)
            => _equipments.TryGetValue(playerId, out var eq) ? eq : null;

        public void Remove(long playerId)
            => _equipments.TryRemove(playerId, out _);
    }
}
