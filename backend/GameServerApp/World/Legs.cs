using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Legs : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Legs;

        public Legs(string id, string name, float weight, string tagName, Position position, int defenseBonus,
                    string description = "", int value = 0)
            : base(id, name, weight, tagName, position, ItemType.Legs, description, value)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
