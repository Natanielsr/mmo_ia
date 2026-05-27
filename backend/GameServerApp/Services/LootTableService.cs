using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

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
        private static readonly (string TagName, double MaxRoll)[] LootTable =
        [
            ("potion",         0.25),
            ("dagger",         0.35),
            ("leather-vest",   0.45),
            ("iron-helmet",    0.57),
            ("wooden-shield",  0.69),
            ("leather-pants",  0.81),
            ("iron-boots",     0.93),
        ];

        private readonly IItemDefinitionRepository _repo;
        private readonly IItemFactory _factory;

        public LootTableService(IItemDefinitionRepository repo, IItemFactory factory)
        {
            _repo = repo;
            _factory = factory;
        }

        public IItem? RollLoot(Position position, string itemId, Func<double>? rng = null)
        {
            var roll = (rng ?? Random.Shared.NextDouble)();

            foreach (var (tagName, maxRoll) in LootTable)
            {
                if (roll < maxRoll)
                {
                    var def = _repo.GetByTagName(tagName);
                    return def is null ? null : _factory.Create(def, itemId, position);
                }
            }

            return null;
        }
    }
}
