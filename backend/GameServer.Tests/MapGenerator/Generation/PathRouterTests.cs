using FractalRiver.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class PathRouterTests
{
    private static float FlatNoise(float x, float y) => 0f;

    [Fact]
    public void Connect_SamePoint_NoPathMarked()
    {
        var (grid, paths) = Setup(10, 10);
        PathRouter.Connect(grid, paths, (5, 5), (5, 5), FlatNoise);
        Assert.DoesNotContain(true, paths.Cast<bool>());
    }

    [Fact]
    public void Connect_ReachableTarget_MarksPath()
    {
        var (grid, paths) = Setup(10, 10);
        PathRouter.Connect(grid, paths, (0, 0), (9, 9), FlatNoise);
        Assert.Contains(true, paths.Cast<bool>());
    }

    [Fact]
    public void Connect_PathIsSubsetOfGrid()
    {
        var (grid, paths) = Setup(15, 15);
        PathRouter.Connect(grid, paths, (0, 0), (14, 14), FlatNoise);
        for (int x = 0; x < 15; x++)
        for (int y = 0; y < 15; y++)
            if (paths[x, y]) Assert.True(grid[x, y]);
    }

    [Fact]
    public void Connect_UnreachableTarget_NoPathMarked()
    {
        // Apenas (0,0) é terra; (4,4) é água — sem caminho possível
        var grid  = new bool[5, 5];
        var paths = new bool[5, 5];
        grid[0, 0] = true;
        PathRouter.Connect(grid, paths, (0, 0), (4, 4), FlatNoise);
        Assert.DoesNotContain(true, paths.Cast<bool>());
    }

    [Fact]
    public void Connect_Deterministic()
    {
        var (gridA, pathsA) = Setup(15, 15);
        var (gridB, pathsB) = Setup(15, 15);
        PathRouter.Connect(gridA, pathsA, (0, 0), (14, 14), FlatNoise);
        PathRouter.Connect(gridB, pathsB, (0, 0), (14, 14), FlatNoise);
        for (int x = 0; x < 15; x++)
        for (int y = 0; y < 15; y++)
            Assert.Equal(pathsA[x, y], pathsB[x, y]);
    }

    private static (bool[,] grid, bool[,] paths) Setup(int w, int h)
    {
        var grid  = new bool[w, h];
        var paths = new bool[w, h];
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            grid[x, y] = true;
        return (grid, paths);
    }
}
