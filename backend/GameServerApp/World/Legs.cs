using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Legs : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Legs;

        public Legs(string id, string name, float weight, Position position, int defenseBonus)
            : base(id, name, weight, position, ItemType.Legs)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
