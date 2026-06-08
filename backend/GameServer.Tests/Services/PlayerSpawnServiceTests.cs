using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GameServer.Tests.Services;

public class PlayerSpawnServiceTests
{
    private const int Cols = 4;
    private const int Rows = 4;
    private const int ChunkSize = 2;
    private const int CenterX = Cols / 2; // 2
    private const int CenterY = Rows / 2; // 2

    private static IOptions<WorldConfig> MakeConfig() => Options.Create(new WorldConfig
    {
        Map = new MapConfig { Width = Cols, Height = Rows, ChunkSize = ChunkSize }
    });

    private static WorldData MakeWorldData() =>
        new(Cols, Rows, new TileType[Cols, Rows], new Biome[Cols, Rows]);

    private static (PlayerSpawnService Svc,
                    Mock<IFractalRiverWorldService> Fractal,
                    Mock<IStaticWorldManager> World,
                    Mock<IChunkObjectGenerator> Gen)
        BuildSut()
    {
        var fractal = new Mock<IFractalRiverWorldService>();
        fractal.Setup(f => f.WorldData).Returns(MakeWorldData());
        fractal.Setup(f => f.IsWaterTile(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

        var world = new Mock<IStaticWorldManager>();
        world.Setup(w => w.IsBlocked(It.IsAny<Position>())).Returns(false);
        world.Setup(w => w.IsChunkLoaded(It.IsAny<ChunkCoord>())).Returns(false);

        var gen = new Mock<IChunkObjectGenerator>();

        var svc = new PlayerSpawnService(fractal.Object, world.Object, gen.Object, MakeConfig());
        return (svc, fractal, world, gen);
    }

    [Fact]
    public void FindSafePosition_WhenCenterIsWater_ReturnsFirstLandTileInRing1()
    {
        var (svc, fractal, _, _) = BuildSut();
        fractal.Setup(f => f.IsWaterTile(CenterX, CenterY)).Returns(true);

        var result = svc.FindSafePosition();

        // Ring-1 first candidate: dy=-1,dx=-1 → (cx-1, cy-1) = (1,1)
        Assert.Equal(new Position(1, 1), result);
    }

    [Fact]
    public void FindSafePosition_WhenPositionIsBlocked_SkipsAndReturnsNextPassable()
    {
        var (svc, _, world, _) = BuildSut();
        world.Setup(w => w.IsBlocked(new Position(CenterX, CenterY))).Returns(true);

        var result = svc.FindSafePosition();

        // (2,2) skipped; first ring-1 candidate (1,1) is passable
        Assert.Equal(new Position(1, 1), result);
    }

    [Fact]
    public void FindSafePosition_GeneratesChunkBeforeCheckingIsBlocked()
    {
        var (svc, _, world, gen) = BuildSut();
        // Chunk containing (2,2) with chunkSize=2: floor(2/2)=1 → ChunkCoord(1,1)
        var spawnChunk = new ChunkCoord(CenterX / ChunkSize, CenterY / ChunkSize);
        world.Setup(w => w.IsChunkLoaded(spawnChunk)).Returns(false);

        svc.FindSafePosition();

        gen.Verify(g => g.GenerateChunk(spawnChunk), Times.AtLeastOnce);
    }

    [Fact]
    public void FindSafePosition_WhenPositionSurroundedByWater_SkipsAndReturnsUnsurroundedTile()
    {
        var (svc, fractal, _, _) = BuildSut();
        // All 4 cardinal neighbors of (2,2) are water → (2,2) is surrounded
        fractal.Setup(f => f.IsWaterTile(1, 2)).Returns(true);
        fractal.Setup(f => f.IsWaterTile(3, 2)).Returns(true);
        fractal.Setup(f => f.IsWaterTile(2, 1)).Returns(true);
        fractal.Setup(f => f.IsWaterTile(2, 3)).Returns(true);

        var result = svc.FindSafePosition();

        // (2,2) skipped; (1,1) has land neighbors (0,1),(1,0),(1,2) → not surrounded
        Assert.Equal(new Position(1, 1), result);
    }

    [Fact]
    public void FindSafePosition_WhenAllTilesAreWater_ReturnsCenterFallback()
    {
        var (svc, fractal, _, _) = BuildSut();
        fractal.Setup(f => f.IsWaterTile(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

        var result = svc.FindSafePosition();

        Assert.Equal(new Position(CenterX, CenterY), result);
    }
}
