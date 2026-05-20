using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Armor : Item
    {
        public int DefenseBonus { get; }

        public Armor(string id, string name, float weight, Position position, int defenseBonus)
            : base(id, name, weight, position, ItemType.Armor)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
