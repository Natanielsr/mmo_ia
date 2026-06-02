using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Services
{
    public class LootTableService : ILootTableService
    {
        private readonly ILootTableRepository _lootRepo;
        private readonly IItemDefinitionRepository _itemRepo;
        private readonly IItemFactory _factory;

        public LootTableService(
            ILootTableRepository lootRepo,
            IItemDefinitionRepository itemRepo,
            IItemFactory factory)
        {
            _lootRepo = lootRepo;
            _itemRepo = itemRepo;
            _factory = factory;
        }

        public IItem? RollLoot(Position position, string itemId, string monsterObjectCode, Func<double>? rng = null)
        {
            var entries = _lootRepo.GetEntriesFor(monsterObjectCode);
            if (entries is null || entries.Count == 0) return null;

            var nextRandom = rng ?? Random.Shared.NextDouble;

            int totalWeight = 0;
            foreach (var e in entries) totalWeight += e.Weight;

            var roll = nextRandom() * 100;

            if (roll > totalWeight) return null;

            double accumulated = 0;
            foreach (var entry in entries)
            {
                accumulated += entry.Weight;
                if (roll < accumulated)
                {
                    var def = _itemRepo.GetByTagName(entry.ItemTag);
                    return def is null ? null : _factory.Create(def, itemId, position);
                }
            }

            return null;
        }
    }
}
