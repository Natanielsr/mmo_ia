using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class HealingPotion : Item
    {
        public int HealAmount { get; } = 20;

        public HealingPotion(string id, Position position) 
            : base(id, "Healing Potion", 0.1f, position, ItemType.Potion)
        {
        }
    }
}
