using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Services.World.MapGenerator.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class TerrainSamplerTests
{
    [Theory]
    [InlineData(10, 10)]
    [InlineData(50, 30)]
    [InlineData(1,   1)]
    public void Sample_GridSize_IsColsPlusOneByRowsPlusOne(int cols, int rows)
    {
        var (grid, paths) = TerrainSampler.Sample(cols, rows, seed: 0, frequency: 0.05f, threshold: -0.5f);
        Assert.Equal(cols + 1, grid.GetLength(0));
        Assert.Equal(rows + 1, grid.GetLength(1));
        Assert.Equal(cols + 1, paths.GetLength(0));
        Assert.Equal(rows + 1, paths.GetLength(1));
    }

    [Fact]
    public void Sample_PathsArrayInitiallyEmpty()
    {
        var (_, paths) = TerrainSampler.Sample(20, 20, seed: 0, frequency: 0.05f, threshold: -0.5f);
        Assert.DoesNotContain(true, paths.Cast<bool>());
    }

    [Fact]
    public void Sample_SameSeed_Deterministic()
    {
        var (a, _) = TerrainSampler.Sample(20, 20, seed: 42, frequency: 0.05f, threshold: -0.5f);
        var (b, _) = TerrainSampler.Sample(20, 20, seed: 42, frequency: 0.05f, threshold: -0.5f);
        for (int x = 0; x <= 20; x++)
        for (int y = 0; y <= 20; y++)
            Assert.Equal(a[x, y], b[x, y]);
    }

    [Fact]
    public void Sample_HigherThreshold_LessLand()
    {
        var (low,  _) = TerrainSampler.Sample(49, 49, seed: 42, frequency: 0.05f, threshold: -0.9f);
        var (high, _) = TerrainSampler.Sample(49, 49, seed: 42, frequency: 0.05f, threshold:  0.9f);
        Assert.True(CountTrue(low) > CountTrue(high));
    }

    [Fact]
    public void Sample_HasBothLandAndWater_WithDefaultThreshold()
    {
        var (grid, _) = TerrainSampler.Sample(49, 49, seed: 42, frequency: 0.05f, threshold: -0.5f);
        int land = CountTrue(grid);
        Assert.True(land > 0 && land < 50 * 50);
    }

    private static int CountTrue(bool[,] g)
    {
        int c = 0;
        for (int x = 0; x < g.GetLength(0); x++)
        for (int y = 0; y < g.GetLength(1); y++)
            if (g[x, y]) c++;
        return c;
    }
}
