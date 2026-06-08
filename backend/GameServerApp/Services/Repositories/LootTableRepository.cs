using System.Text.Json;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;

namespace GameServerApp.Services.Repositories
{
    public class LootTableRepository : ILootTableRepository
    {
        private readonly Dictionary<string, List<LootEntry>> _tables;

        public LootTableRepository(string jsonContent)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var catalog = JsonSerializer.Deserialize<LootTableCatalog>(jsonContent, options)
                ?? throw new InvalidOperationException("Failed to deserialize loot table catalog.");

            _tables = catalog.LootTables.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(e => new LootEntry(e.ItemTag, e.Weight)).ToList());
        }

        public IReadOnlyList<LootEntry>? GetEntriesFor(string monsterObjectCode) =>
            _tables.TryGetValue(monsterObjectCode, out var entries) ? entries : null;

        private sealed class LootTableCatalog
        {
            public Dictionary<string, List<LootEntryJson>> LootTables { get; init; } = [];
        }

        private sealed class LootEntryJson
        {
            public string ItemTag { get; init; } = string.Empty;
            public int Weight { get; init; }
        }
    }
}
