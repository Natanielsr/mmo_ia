using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class HealingPotion : Item
    {
        public int HealAmount { get; }

        public HealingPotion(string id, string name, float weight, Position position,
                             int healAmount = 20,
                             string description = "", int value = 0, string tagName = "")
            : base(id, name, weight, position, ItemType.Potion, description, value, tagName)
        {
            HealAmount = healAmount;
        }
    }
}
