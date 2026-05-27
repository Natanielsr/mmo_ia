using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class HealingPotion : Item
    {
        public int HealAmount { get; }

        public HealingPotion(string id, string name, float weight, string tagName, Position position,
                             int healAmount = 20,
                             string description = "", int value = 0)
            : base(id, name, weight, tagName, position, ItemType.Potion, description, value)
        {
            HealAmount = healAmount;
        }
    }
}
