using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServerApp.Services
{
    // Ranges de drop (cumulativos):
    //   0.00–0.25 → HealingPotion (25%)
    //   0.25–0.35 → Weapon        (10%)
    //   0.35–0.45 → Armor         (10%)
    //   0.45–0.57 → Helmet        (12%)
    //   0.57–0.69 → Shield        (12%)
    //   0.69–0.81 → Legs          (12%)
    //   0.81–0.93 → Boots         (12%)
    //   0.93–1.00 → nenhum drop   (7%)
    public class LootTableService : ILootTableService
    {
        public IItem? RollLoot(Position position, string itemId, Func<double>? rng = null)
        {
            var roll = (rng ?? Random.Shared.NextDouble)();

            if (roll < 0.25)
                return new HealingPotion(itemId, position);

            if (roll < 0.35)
                return new Weapon(itemId, "Iron Sword", 1.5f, position, attackBonus: 5);

            if (roll < 0.45)
                return new Armor(itemId, "Leather Vest", 2.0f, position, defenseBonus: 3);

            if (roll < 0.57)
                return new Helmet(itemId, "Iron Helmet", 1.2f, position, defenseBonus: 4);

            if (roll < 0.69)
                return new Shield(itemId, "Wooden Shield", 2.5f, position, defenseBonus: 3);

            if (roll < 0.81)
                return new Legs(itemId, "Leather Pants", 1.0f, position, defenseBonus: 2);

            if (roll < 0.93)
                return new Boots(itemId, "Iron Boots", 1.5f, position, defenseBonus: 2);

            return null;
        }
    }
}
