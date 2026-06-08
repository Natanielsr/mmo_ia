namespace GameServerApp.Contracts.Services.Repositories
{
    public record LootEntry(string ItemTag, int Weight);

    public interface ILootTableRepository
    {
        IReadOnlyList<LootEntry>? GetEntriesFor(string monsterObjectCode);
    }
}
