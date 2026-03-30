using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class ProceduralWorldServiceTests
{
    [Fact]
    public void GenerateRandomObstacles_ShouldNotGenerateInsideSafeSpawnArea()
    {
        // Arrange
        var idGenerator = new FakeIdGeneratorService();
        var service = new ProceduralWorldService(idGenerator, new FakeStaticWorldManager());

        // Act
        var obstacles = service.GenerateRandomObstacles(
            width: 30,
            height: 30,
            fillPercentage: 0.15,
            safeSpawnRadius: 3,
            seed: 123);

        // Assert
        Assert.NotEmpty(obstacles);
        Assert.DoesNotContain(obstacles, o =>
            Math.Abs(o.Position.X) <= 3 &&
            Math.Abs(o.Position.Y) <= 3);
    }

    [Fact]
    public void GenerateRandomObstacles_WithSameSeed_ShouldGenerateSamePositions()
    {
        // Arrange
        var serviceA = new ProceduralWorldService(new FakeIdGeneratorService(), new FakeStaticWorldManager());
        var serviceB = new ProceduralWorldService(new FakeIdGeneratorService(), new FakeStaticWorldManager());

        // Act
        var obstaclesA = serviceA.GenerateRandomObstacles(20, 20, 0.10, 2, seed: 999);
        var obstaclesB = serviceB.GenerateRandomObstacles(20, 20, 0.10, 2, seed: 999);

        var positionsA = obstaclesA.Select(o => o.Position).OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        var positionsB = obstaclesB.Select(o => o.Position).OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        // Assert
        Assert.Equal(positionsA, positionsB);
    }

    [Fact]
    public void GenerateRandomObstacles_ShouldGenerateUniquePositions()
    {
        // Arrange
        var service = new ProceduralWorldService(new FakeIdGeneratorService(), new FakeStaticWorldManager());

        // Act
        var obstacles = service.GenerateRandomObstacles(25, 25, 0.20, 1, seed: 777);
        var uniquePositionCount = obstacles.Select(o => o.Position).Distinct().Count();

        // Assert
        Assert.Equal(obstacles.Count, uniquePositionCount);
    }

    private sealed class FakeIdGeneratorService : IIdGeneratorService
    {
        private long _nextId = 1;

        public long GenerateId() => _nextId++;
    }

    private sealed class FakeStaticWorldManager : IStaticWorldManager
    {
        public void AddStaticObject(IStaticWorldObject staticObject)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<Position, IStaticWorldObject> GetAllObjects()
        {
            throw new NotImplementedException();
        }

        public IStaticWorldObject? GetObjectAt(Position position)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IStaticWorldObject> GetObjectsInArea(Position topLeft, Position bottomRight)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Position position)
        {
            throw new NotImplementedException();
        }

        public bool IsPassable(Position position)
        {
            throw new NotImplementedException();
        }

        public bool IsChunkLoaded(ChunkCoord coord)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IStaticWorldObject> GetChunkObjects(ChunkCoord coord)
        {
            throw new NotImplementedException();
        }

        public bool RemoveObjectAt(Position position)
        {
            throw new NotImplementedException();
        }
    }
}
