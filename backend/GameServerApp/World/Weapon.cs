using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Weapon : Item
    {
        public int AttackBonus { get; }

        public Weapon(string id, string name, float weight, Position position, int attackBonus)
            : base(id, name, weight, position, ItemType.Weapon)
        {
            AttackBonus = attackBonus;
        }
    }
}
