using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Armor : Item, IDefensiveItem
    {
        public int DefenseBonus { get; }
        private readonly EquipmentSlot _slot;
        public override EquipmentSlot? Slot => _slot;

        public Armor(string id, string name, float weight, Position position, int defenseBonus,
                     EquipmentSlot slot = EquipmentSlot.Chest,
                     string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Armor, description, value, tagName)
        {
            DefenseBonus = defenseBonus;
            _slot = slot;
        }
    }
}
