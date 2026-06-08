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

public class BiomeDefinitionRepository : IBiomeDefinitionRepository
{
    private readonly Dictionary<int, BiomeDefinition> _byId;
    private readonly Dictionary<string, BiomeDefinition> _byTagName;
    private readonly BiomeDefinition _default;

    public BiomeDefinitionRepository(string jsonContent)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var catalog = JsonSerializer.Deserialize<BiomeCatalog>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize biome catalog.");

        if (catalog.Biomes.Count == 0)
            throw new InvalidOperationException("Biome catalog is empty.");

        _byId = catalog.Biomes.ToDictionary(x => x.Id);
        _byTagName = catalog.Biomes.ToDictionary(x => x.TagName);
        _default = catalog.Biomes.FirstOrDefault(b => b.IsDefault)
            ?? catalog.Biomes[0];
    }

    public BiomeDefinition? GetById(int id) =>
        _byId.TryGetValue(id, out var def) ? def : null;

    public BiomeDefinition? GetByTagName(string tagName) =>
        _byTagName.TryGetValue(tagName, out var def) ? def : null;

    public IReadOnlyList<BiomeDefinition> GetAll() =>
        _byId.Values.ToList().AsReadOnly();

    public BiomeDefinition GetDefault() => _default;

    private record BiomeCatalog(List<BiomeDefinition> Biomes);
}
