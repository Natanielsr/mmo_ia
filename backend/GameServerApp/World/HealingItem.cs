using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class HealingItem : Item
    {
        public int HealAmount { get; }

        public HealingItem(string id, string name, float weight, string tagName,
                           Position position, ItemType type, int healAmount = 10,
                           string description = "", int value = 0)
            : base(id, name, weight, tagName, position, type, description, value)
        {
            HealAmount = healAmount;
        }
    }
}
