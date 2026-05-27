using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IMonsterDefinitionRepository
{
    MonsterDefinition? GetById(int id);
    MonsterDefinition? GetByTagName(string tagName);
    IReadOnlyList<MonsterDefinition> GetAll();
}
