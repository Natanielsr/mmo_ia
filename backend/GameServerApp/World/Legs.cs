using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Legs : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Legs;

        public Legs(string id, string name, float weight, Position position, int defenseBonus,
                    string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Legs, description, value, tagName)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
