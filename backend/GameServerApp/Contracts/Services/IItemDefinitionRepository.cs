using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IItemDefinitionRepository
{
    ItemDefinition? GetById(int id);
    ItemDefinition? GetByTagName(string tagName);
    IReadOnlyList<ItemDefinition> GetAll();
}
