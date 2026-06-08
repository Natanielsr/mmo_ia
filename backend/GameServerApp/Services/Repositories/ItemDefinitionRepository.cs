using System.Text.Json;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Dtos;

namespace GameServerApp.Services.Repositories;

public class ItemDefinitionRepository : IItemDefinitionRepository
{
    private readonly Dictionary<int, ItemDefinition> _byId;
    private readonly Dictionary<string, ItemDefinition> _byTagName;

    public ItemDefinitionRepository(string jsonContent)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var catalog = JsonSerializer.Deserialize<ItemCatalog>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize item catalog.");

        _byId = catalog.Items.ToDictionary(x => x.Id);
        _byTagName = catalog.Items.ToDictionary(x => x.TagName);
    }

    public ItemDefinition? GetById(int id) =>
        _byId.TryGetValue(id, out var def) ? def : null;

    public ItemDefinition? GetByTagName(string tagName) =>
        _byTagName.TryGetValue(tagName, out var def) ? def : null;

    public IReadOnlyList<ItemDefinition> GetAll() =>
        _byId.Values.ToList().AsReadOnly();

    private record ItemCatalog(List<ItemDefinition> Items);
}
