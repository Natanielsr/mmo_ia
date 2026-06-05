using FractalRiver.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class GridUtilitiesTests
{
    [Fact]
    public void Adj4_CornerCell_ReturnsTwoNeighbors()
    {
        Assert.Equal(2, GridUtilities.Adj4(0, 0, 5, 5).Count());
    }

    [Fact]
    public void Adj4_EdgeCell_ReturnsThreeNeighbors()
    {
        Assert.Equal(3, GridUtilities.Adj4(0, 2, 5, 5).Count());
    }

    [Fact]
    public void Adj4_InteriorCell_ReturnsFourNeighbors()
    {
        Assert.Equal(4, GridUtilities.Adj4(2, 2, 5, 5).Count());
    }

    [Fact]
    public void Adj4_AllNeighborsInBounds()
    {
        int w = 5, h = 5;
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            foreach (var (nx, ny) in GridUtilities.Adj4(x, y, w, h))
            {
                Assert.InRange(nx, 0, w - 1);
                Assert.InRange(ny, 0, h - 1);
            }
    }

    [Fact]
    public void Adj4_SingleCellGrid_ReturnsNoNeighbors()
    {
        Assert.Empty(GridUtilities.Adj4(0, 0, 1, 1));
    }

    [Fact]
    public void Adj4_NoDuplicates()
    {
        var neighbors = GridUtilities.Adj4(2, 2, 5, 5).ToList();
        Assert.Equal(neighbors.Count, neighbors.Distinct().Count());
    }
}
