using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IProceduralWorldService
{
    IReadOnlyCollection<IStaticWorldObject> GenerateRandomObstacles(
        int width,
        int height,
        double fillPercentage,
        int safeSpawnRadius,
        int? seed = null);
    IReadOnlyCollection<MapObjectData> GetAllMapObjects();
}
