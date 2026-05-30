using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Cheese : Item
    {
        public int HealPerTick { get; }
        public int TotalTicks { get; }

        public Cheese(string id, string name, float weight, string tagName, Position position,
                      int healAmount = 10, string description = "", int value = 0)
            : base(id, name, weight, tagName, position, ItemType.Food, description, value)
        {
            HealPerTick = Math.Max(1, healAmount / 10);
            TotalTicks = 10;
        }
    }
}
