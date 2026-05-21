using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Boots : Item
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Boots;

        public Boots(string id, string name, float weight, Position position, int defenseBonus)
            : base(id, name, weight, position, ItemType.Boots)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
