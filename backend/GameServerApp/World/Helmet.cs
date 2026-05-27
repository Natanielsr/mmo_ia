using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Helmet : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Helmet;

        public Helmet(string id, string name, float weight, Position position, int defenseBonus,
                      string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Helmet, description, value, tagName)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
