using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Weapon : Item
    {
        public int AttackBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Weapon;

        public Weapon(string id, string name, float weight, string tagName, Position position, int attackBonus,
                      string description = "", int value = 0)
            : base(id, name, weight, tagName, position, ItemType.Weapon, description, value)
        {
            AttackBonus = attackBonus;
        }
    }
}
