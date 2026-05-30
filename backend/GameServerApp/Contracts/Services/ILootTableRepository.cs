namespace GameServerApp.Contracts.Services
{
    public record LootEntry(string ItemTag, int Weight);

    public interface ILootTableRepository
    {
        IReadOnlyList<LootEntry>? GetEntriesFor(string monsterObjectCode);
    }
}
