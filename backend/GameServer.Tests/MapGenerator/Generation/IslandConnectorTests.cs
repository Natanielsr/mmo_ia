using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Services.World.MapGenerator.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class IslandConnectorTests
{
    [Fact]
    public void Connect_SingleIsland_NoPathsAdded()
    {
        var grid  = AllTrue(10, 10);
        var paths = new bool[10, 10];
        var (labels, count) = ConnectivityAnalyzer.Label(grid);

        IslandConnector.Connect(grid, paths, labels, count, seed: 0);

        Assert.DoesNotContain(true, paths.Cast<bool>());
    }

    [Fact]
    public void Connect_TwoIsolatedIslands_PathsExist()
    {
        var grid  = new bool[20, 20];
        var paths = new bool[20, 20];
        FillRect(grid, 0, 0, 3, 3);
        FillRect(grid, 16, 16, 3, 3);

        var (labels, count) = ConnectivityAnalyzer.Label(grid);
        IslandConnector.Connect(grid, paths, labels, count, seed: 0);

        Assert.Contains(true, paths.Cast<bool>());
    }

    [Fact]
    public void Connect_PathsAreSubsetOfGrid()
    {
        var grid  = new bool[20, 20];
        var paths = new bool[20, 20];
        FillRect(grid, 0, 0, 3, 3);
        FillRect(grid, 16, 16, 3, 3);

        var (labels, count) = ConnectivityAnalyzer.Label(grid);
        IslandConnector.Connect(grid, paths, labels, count, seed: 0);

        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
            if (paths[x, y]) Assert.True(grid[x, y]);
    }

    [Fact]
    public void Connect_ZeroComponents_NoException()
    {
        var grid   = new bool[5, 5];
        var paths  = new bool[5, 5];
        var labels = new int[5, 5];

        var ex = Record.Exception(() => IslandConnector.Connect(grid, paths, labels, count: 0, seed: 0));
        Assert.Null(ex);
    }

    [Fact]
    public void Connect_Deterministic_SameSeed()
    {
        var gridA  = new bool[20, 20]; var pathsA = new bool[20, 20];
        var gridB  = new bool[20, 20]; var pathsB = new bool[20, 20];
        FillRect(gridA, 0, 0, 3, 3);  FillRect(gridA, 16, 16, 3, 3);
        FillRect(gridB, 0, 0, 3, 3);  FillRect(gridB, 16, 16, 3, 3);

        var (labelsA, countA) = ConnectivityAnalyzer.Label(gridA);
        var (labelsB, countB) = ConnectivityAnalyzer.Label(gridB);
        IslandConnector.Connect(gridA, pathsA, labelsA, countA, seed: 42);
        IslandConnector.Connect(gridB, pathsB, labelsB, countB, seed: 42);

        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
            Assert.Equal(pathsA[x, y], pathsB[x, y]);
    }

    private static bool[,] AllTrue(int w, int h)
    {
        var g = new bool[w, h];
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            g[x, y] = true;
        return g;
    }

    private static void FillRect(bool[,] g, int ox, int oy, int w, int h)
    {
        for (int x = ox; x < ox + w; x++)
        for (int y = oy; y < oy + h; y++)
            g[x, y] = true;
    }
}
