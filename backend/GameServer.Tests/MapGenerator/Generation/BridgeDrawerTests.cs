using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Services.World.MapGenerator.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class BridgeDrawerTests
{
    [Fact]
    public void Draw_MarksAtLeastOneCell()
    {
        var (grid, paths) = Setup(20, 20);
        BridgeDrawer.Draw(grid, paths, (0, 0), (15, 15), new Random(0));
        Assert.Contains(true, paths.Cast<bool>());
    }

    [Fact]
    public void Draw_PathsIsSubsetOfGrid()
    {
        var (grid, paths) = Setup(20, 20);
        BridgeDrawer.Draw(grid, paths, (0, 0), (15, 15), new Random(42));
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
            if (paths[x, y]) Assert.True(grid[x, y]);
    }

    [Fact]
    public void Draw_ShortPath_MarksIntermediateCells()
    {
        // len ≤ 4 → L-path direto
        var (grid, paths) = Setup(10, 10);
        BridgeDrawer.Draw(grid, paths, (1, 1), (3, 1), new Random(0));
        Assert.True(paths[2, 1], "Célula intermediária da ponte curta deve ser marcada.");
    }

    [Fact]
    public void Draw_AllMarkedCellsWithinBounds()
    {
        int w = 15, h = 15;
        var (grid, paths) = Setup(w, h);
        BridgeDrawer.Draw(grid, paths, (1, 1), (12, 12), new Random(7));
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            Assert.InRange(x, 0, w - 1);
            Assert.InRange(y, 0, h - 1);
        }
    }

    [Fact]
    public void Draw_Deterministic_SameSeed()
    {
        var (gridA, pathsA) = Setup(20, 20);
        var (gridB, pathsB) = Setup(20, 20);
        BridgeDrawer.Draw(gridA, pathsA, (0, 5), (18, 14), new Random(99));
        BridgeDrawer.Draw(gridB, pathsB, (0, 5), (18, 14), new Random(99));
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
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
