using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Managers;

public interface IMonsterManager
{
    IReadOnlyCollection<IMonster> SpawnRandomMonsters(
        int count,
        int width,
        int height,
        int safeSpawnRadius,
        int? seed = null);

    IReadOnlyCollection<IMonster> GetAllMonsters();
    IMonster? GetMonsterById(long id);
    bool RemoveMonster(long monsterId);
}
