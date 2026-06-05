using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace GameServer.Tests.Services;

public class FractalRiverWorldServiceTests
{
    private static FractalRiverWorldService CreateService(int width = 32, int height = 24)
    {
        var config = Options.Create(new WorldConfig
        {
            Map = new MapConfig { Width = width, Height = height, GeneratorSeed = 42, GeneratorFrequency = 0.05f }
        });
        return new FractalRiverWorldService(config);
    }

    [Fact]
    public void GetWorldMap_Dimensions_MatchConfiguredSize()
    {
        var service = CreateService(width: 32, height: 24);

        var map = service.GetWorldMap();

        Assert.Equal(32, map.Cols);
        Assert.Equal(24, map.Rows);
        Assert.Equal(24, map.Tiles.Length);   // rows
        Assert.Equal(32, map.Tiles[0].Length); // cols
        Assert.Equal(24, map.Biomes.Length);
        Assert.Equal(32, map.Biomes[0].Length);
    }

    [Fact]
    public void GetWorldMap_TilesAndBiomes_MatchWorldDataMatrix()
    {
        var service = CreateService(width: 16, height: 16);
        var map = service.GetWorldMap();
        var data = service.WorldData;

        for (int r = 0; r < map.Rows; r++)
            for (int c = 0; c < map.Cols; c++)
            {
                Assert.Equal((int)data.Tiles[c, r], map.Tiles[r][c]);
                Assert.Equal((int)data.Biomes[c, r], map.Biomes[r][c]);
            }
    }

    [Fact]
    public void GetWorldMap_TileValues_AreValidTileTypeOrdinals()
    {
        var service = CreateService();
        var map = service.GetWorldMap();

        int maxTile = (int)TileType.PathTerrainCrossNEtoSW; // last enum value
        foreach (var row in map.Tiles)
            foreach (var t in row)
                Assert.InRange(t, 0, maxTile);
    }
}
