using FractalRiver.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class ConnectivityAnalyzerTests
{
    [Fact]
    public void Label_AllLand_SingleComponent()
    {
        var (labels, count) = ConnectivityAnalyzer.Label(AllTrue(5, 5));
        Assert.Equal(1, count);
        int expected = labels[0, 0];
        for (int x = 0; x < 5; x++)
        for (int y = 0; y < 5; y++)
            Assert.Equal(expected, labels[x, y]);
    }

    [Fact]
    public void Label_AllWater_ZeroComponents()
    {
        var (labels, count) = ConnectivityAnalyzer.Label(new bool[5, 5]);
        Assert.Equal(0, count);
        for (int x = 0; x < 5; x++)
        for (int y = 0; y < 5; y++)
            Assert.Equal(0, labels[x, y]);
    }

    [Fact]
    public void Label_TwoIsolatedCells_TwoComponents()
    {
        var grid = new bool[5, 5];
        grid[0, 0] = true;
        grid[4, 4] = true;
        var (_, count) = ConnectivityAnalyzer.Label(grid);
        Assert.Equal(2, count);
    }

    [Fact]
    public void Label_TwoIsolatedCells_HaveDifferentLabels()
    {
        var grid = new bool[5, 5];
        grid[0, 0] = true;
        grid[4, 4] = true;
        var (labels, _) = ConnectivityAnalyzer.Label(grid);
        Assert.NotEqual(labels[0, 0], labels[4, 4]);
    }

    [Fact]
    public void Label_ConnectedCells_HaveSameLabel()
    {
        var grid = new bool[5, 5];
        grid[1, 1] = true;
        grid[1, 2] = true;
        grid[1, 3] = true;
        var (labels, _) = ConnectivityAnalyzer.Label(grid);
        Assert.Equal(labels[1, 1], labels[1, 2]);
        Assert.Equal(labels[1, 2], labels[1, 3]);
    }

    [Fact]
    public void Label_OutputSize_MatchesInput()
    {
        var (labels, _) = ConnectivityAnalyzer.Label(AllTrue(7, 3));
        Assert.Equal(7, labels.GetLength(0));
        Assert.Equal(3, labels.GetLength(1));
    }

    [Fact]
    public void Label_WaterCells_AlwaysLabeledZero()
    {
        var grid = new bool[5, 5];
        grid[2, 2] = true;
        var (labels, _) = ConnectivityAnalyzer.Label(grid);
        for (int x = 0; x < 5; x++)
        for (int y = 0; y < 5; y++)
            if (!grid[x, y]) Assert.Equal(0, labels[x, y]);
    }

    private static bool[,] AllTrue(int w, int h)
    {
        var g = new bool[w, h];
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            g[x, y] = true;
        return g;
    }
}
