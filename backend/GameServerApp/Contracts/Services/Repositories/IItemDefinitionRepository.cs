using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services.Repositories;

public interface IItemDefinitionRepository
{
    ItemDefinition? GetById(int id);
    ItemDefinition? GetByTagName(string tagName);
    IReadOnlyList<ItemDefinition> GetAll();
}
