using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Weapon : Item
    {
        public int AttackBonus { get; }
        public override EquipmentSlot? Slot => EquipmentSlot.Weapon;

        public Weapon(string id, string name, float weight, Position position, int attackBonus,
                      string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Weapon, description, value, tagName)
        {
            AttackBonus = attackBonus;
        }
    }
}
