using GameServerApp.Services.World.MapGenerator;
using Xunit;

namespace FractalRiver.Tests;

public class TerrainBorderTests
{
    [Theory]
    [InlineData(20, 20, 42)]
    [InlineData(32, 32, 0)]
    [InlineData(10, 15, 999)]
    public void GenerateWorld_BorderTiles_AreAllWaterFull(int cols, int rows, int seed)
    {
        var world = TerrainGenerator.GenerateWorld(cols, rows, seed, frequency: 0.05f);

        for (int y = 0; y < rows; y++)
        {
            Assert.Equal(TileType.WaterFull, world.Tiles[0, y]);
            Assert.Equal(TileType.WaterFull, world.Tiles[cols - 1, y]);
        }
        for (int x = 0; x < cols; x++)
        {
            Assert.Equal(TileType.WaterFull, world.Tiles[x, 0]);
            Assert.Equal(TileType.WaterFull, world.Tiles[x, rows - 1]);
        }
    }

    [Theory]
    [InlineData(20, 20, 42)]
    [InlineData(32, 32, 0)]
    public void GenerateWorld_InteriorTiles_AreNotAllWater(int cols, int rows, int seed)
    {
        var world = TerrainGenerator.GenerateWorld(cols, rows, seed, frequency: 0.05f);

        int nonWater = 0;
        for (int x = 1; x < cols - 1; x++)
        for (int y = 1; y < rows - 1; y++)
            if (world.Tiles[x, y] != TileType.WaterFull) nonWater++;

        Assert.True(nonWater > 0, "O interior do mapa não deve ser todo água.");
    }
}
