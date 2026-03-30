using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Managers;

public interface IMonsterManager
{
    IReadOnlyCollection<IMonster> SpawnRandomMonsters(
        int count,
        int width,
        int height,
        int safeSpawnRadius,
        int? seed = null);

    IReadOnlyCollection<IMonster> SpawnMonstersNearPosition(
        int count,
        Position center,
        int minRadius,
        int maxRadius,
        int? seed = null);

    IReadOnlyCollection<IMonster> GetAllMonsters();
    IReadOnlyCollection<MonsterData> GetAllMonstersAsData();
    IMonster? GetMonsterById(long id);
    MonsterData? GetMonsterDataById(long id);
    bool RemoveMonster(long monsterId);
}
