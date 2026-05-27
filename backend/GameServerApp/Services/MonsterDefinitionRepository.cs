using System.Text.Json;
using GameServerApp.Contracts.Services;
using GameServerApp.Dtos;

namespace GameServerApp.Services;

public class MonsterDefinitionRepository : IMonsterDefinitionRepository
{
    private readonly Dictionary<int, MonsterDefinition> _byId;
    private readonly Dictionary<string, MonsterDefinition> _byTagName;

    public MonsterDefinitionRepository(string jsonContent)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var catalog = JsonSerializer.Deserialize<MonsterCatalog>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize monster catalog.");

        _byId = catalog.Monsters.ToDictionary(x => x.Id);
        _byTagName = catalog.Monsters.ToDictionary(x => x.TagName);
    }

    public MonsterDefinition? GetById(int id) =>
        _byId.TryGetValue(id, out var def) ? def : null;

    public MonsterDefinition? GetByTagName(string tagName) =>
        _byTagName.TryGetValue(tagName, out var def) ? def : null;

    public IReadOnlyList<MonsterDefinition> GetAll() =>
        _byId.Values.ToList().AsReadOnly();

    private record MonsterCatalog(List<MonsterDefinition> Monsters);
}
