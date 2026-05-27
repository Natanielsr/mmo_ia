using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Shield : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Shield;

        public Shield(string id, string name, float weight, Position position, int defenseBonus,
                      string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Shield, description, value, tagName)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
