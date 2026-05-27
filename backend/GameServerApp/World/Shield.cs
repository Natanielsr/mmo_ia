using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Shield : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Shield;

        public Shield(string id, string name, float weight, string tagName, Position position, int defenseBonus,
                      string description = "", int value = 0)
            : base(id, name, weight, tagName, position, ItemType.Shield, description, value)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
