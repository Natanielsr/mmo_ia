using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServerApp.Services
{
    // Ranges de drop (cumulativos):
    //   0.00–0.30 → HealingPotion (30%)
    //   0.30–0.40 → Weapon        (10%)
    //   0.40–0.50 → Armor         (10%)
    //   0.50–1.00 → nenhum drop   (50%)
    public class LootTableService : ILootTableService
    {
        public IItem? RollLoot(Position position, string itemId, Func<double>? rng = null)
        {
            var roll = (rng ?? Random.Shared.NextDouble)();

            if (roll < 0.30)
                return new HealingPotion(itemId, position);

            if (roll < 0.40)
                return new Weapon(itemId, "Iron Sword", 1.5f, position, attackBonus: 5);

            if (roll < 0.50)
                return new Armor(itemId, "Leather Vest", 2.0f, position, defenseBonus: 3);

            return null;
        }
    }
}
