using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World;

public class ProceduralWorldService : IProceduralWorldService
{
    private static readonly string[] ObstacleNames = ["Tree", "Rock", "Bush", "Pillar"];
    private readonly IIdGeneratorService _idGeneratorService;

    private static readonly Position PlayserSpawnPosition = new Position(0, 0);

    public ProceduralWorldService(IIdGeneratorService idGeneratorService)
    {
        _idGeneratorService = idGeneratorService;
    }

    public IReadOnlyCollection<IStaticWorldObject> GenerateRandomObstacles(
        int width,
        int height,
        double fillPercentage,
        int safeSpawnRadius,
        int? seed = null)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (fillPercentage <= 0 || fillPercentage >= 1) throw new ArgumentOutOfRangeException(nameof(fillPercentage));
        if (safeSpawnRadius < 0) throw new ArgumentOutOfRangeException(nameof(safeSpawnRadius));

        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        var minX = -(width / 2);
        var maxX = minX + width - 1;
        var minY = -(height / 2);
        var maxY = minY + height - 1;

        var targetCount = (int)Math.Floor(width * height * fillPercentage);
        var generated = new List<IStaticWorldObject>(targetCount);
        var occupiedPositions = new HashSet<Position>();

        var maxAttempts = targetCount * 20;
        var attempts = 0;

        while (generated.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            var x = rng.Next(minX, maxX + 1);
            var y = rng.Next(minY, maxY + 1);
            var pos = new Position(x, y);

            if (pos == PlayserSpawnPosition) //player spawn position
                continue;

            var insideSpawnSafeArea = Math.Abs(x) <= safeSpawnRadius && Math.Abs(y) <= safeSpawnRadius;
            if (insideSpawnSafeArea || !occupiedPositions.Add(pos))
            {
                continue;
            }

            var obstacleName = ObstacleNames[rng.Next(ObstacleNames.Length)];
            generated.Add(new StaticObject(
                id: _idGeneratorService.GenerateId(),
                position: pos,
                name: obstacleName,
                isPassable: false));
        }

        return generated;
    }
}
