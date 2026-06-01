using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IBiomeDefinitionRepository
{
    BiomeDefinition? GetById(int id);
    BiomeDefinition? GetByTagName(string tagName);
    IReadOnlyList<BiomeDefinition> GetAll();
    BiomeDefinition GetDefault();
}
