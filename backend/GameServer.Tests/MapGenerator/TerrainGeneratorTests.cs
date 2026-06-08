using GameServerApp.Services.World.MapGenerator;
using Xunit;

namespace FractalRiver.Tests;

public class TerrainGeneratorTests
{
    // ── Dimensões ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(10,  10)]
    [InlineData(100, 80)]
    [InlineData(1,   1)]
    public void Generate_GridSize_IsColsPlusOneByRowsPlusOne(int cols, int rows)
    {
        var (grid, _) = TerrainGenerator.Generate(cols, rows, seed: 0, frequency: 0.05f);

        Assert.Equal(cols + 1, grid.GetLength(0));
        Assert.Equal(rows + 1, grid.GetLength(1));
    }

    // ── Determinismo ──────────────────────────────────────────────────────────

    [Fact]
    public void Generate_SameSeed_ProducesSameGrid()
    {
        var (a, _) = TerrainGenerator.Generate(50, 50, seed: 42, frequency: 0.05f);
        var (b, _) = TerrainGenerator.Generate(50, 50, seed: 42, frequency: 0.05f);

        for (int x = 0; x <= 50; x++)
        for (int y = 0; y <= 50; y++)
            Assert.Equal(a[x, y], b[x, y]);
    }

    [Fact]
    public void Generate_DifferentSeeds_ProduceDifferentGrids()
    {
        var (a, _) = TerrainGenerator.Generate(50, 50, seed: 1, frequency: 0.05f);
        var (b, _) = TerrainGenerator.Generate(50, 50, seed: 2, frequency: 0.05f);

        int differences = 0;
        for (int x = 0; x <= 50; x++)
        for (int y = 0; y <= 50; y++)
            if (a[x, y] != b[x, y]) differences++;

        Assert.True(differences > 0, "Seeds diferentes devem gerar grades diferentes.");
    }

    // ── Conteúdo ──────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_DefaultThreshold_MostlyLand()
    {
        var (grid, _) = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f);
        int total = 100 * 100;
        int land  = CountLand(grid, 99, 99);
        double ratio = (double)land / total;

        Assert.True(ratio > 0.5, $"Esperado >50% terra, obtido {ratio:P1}");
    }

    [Fact]
    public void Generate_HasSomeWater()
    {
        var (grid, _) = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f);
        int water = CountWater(grid, 99, 99);

        Assert.True(water > 0, "Deve existir ao menos uma célula de água.");
    }

    [Fact]
    public void Generate_HighThreshold_MoreWater()
    {
        var (grid, _) = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f, threshold: 0.9f);
        int land = CountLand(grid, 99, 99);

        Assert.True(land < 100 * 100 / 2, "Threshold alto deve resultar em menos de 50% de terra.");
    }

    [Fact]
    public void Generate_LowThreshold_MoreLand()
    {
        var (gridLow, _)  = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f, threshold: -0.9f);
        var (gridBase, _) = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f, threshold: -0.5f);

        int landLow  = CountLand(gridLow,  99, 99);
        int landBase = CountLand(gridBase, 99, 99);

        Assert.True(landLow >= landBase, "Threshold menor deve produzir ao menos tanta terra quanto threshold maior.");
    }

    // ── Frequência ────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_DifferentFrequencies_ProduceDifferentPatterns()
    {
        var (lowFreq, _)  = TerrainGenerator.Generate(50, 50, seed: 42, frequency: 0.02f);
        var (highFreq, _) = TerrainGenerator.Generate(50, 50, seed: 42, frequency: 0.15f);

        int differences = 0;
        for (int x = 0; x <= 50; x++)
        for (int y = 0; y <= 50; y++)
            if (lowFreq[x, y] != highFreq[x, y]) differences++;

        Assert.True(differences > 0, "Frequências diferentes devem alterar o padrão do mapa.");
    }

    // ── Casos extremos ────────────────────────────────────────────────────────

    [Fact]
    public void Generate_MinimalGrid_DoesNotThrow()
    {
        var ex = Record.Exception(() => TerrainGenerator.Generate(1, 1, seed: 0, frequency: 0.05f));
        Assert.Null(ex);
    }

    [Fact]
    public void Generate_ZeroSeed_Works()
    {
        var ex = Record.Exception(() => TerrainGenerator.Generate(10, 10, seed: 0, frequency: 0.05f));
        Assert.Null(ex);
    }

    [Fact]
    public void Generate_NegativeSeed_Works()
    {
        var ex = Record.Exception(() => TerrainGenerator.Generate(10, 10, seed: -1, frequency: 0.05f));
        Assert.Null(ex);
    }

    // ── Conectividade ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(42,  0.05f)]
    [InlineData(1,   0.10f)]
    [InlineData(999, 0.08f)]
    public void Generate_AllLandConnected_NeverIsolatedIsland(int seed, float freq)
    {
        var (grid, _) = TerrainGenerator.Generate(99, 99, seed, freq);

        Assert.Equal(1, CountLandComponents(grid));
    }

    [Fact]
    public void Generate_HighFrequency_AllLandConnected()
    {
        var (grid, _) = TerrainGenerator.Generate(99, 99, seed: 7, frequency: 0.15f);

        Assert.Equal(1, CountLandComponents(grid));
    }

    [Fact]
    public void Generate_SmallGrid_AllLandConnected()
    {
        var (grid, _) = TerrainGenerator.Generate(10, 10, seed: 0, frequency: 0.05f);

        Assert.Equal(1, CountLandComponents(grid));
    }

    // ── Caminhos ──────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_PathIsSubsetOfLand()
    {
        var (isLand, isPath) = TerrainGenerator.Generate(99, 99, seed: 42, frequency: 0.05f);

        for (int x = 0; x <= 99; x++)
        for (int y = 0; y <= 99; y++)
            if (isPath[x, y])
                Assert.True(isLand[x, y], $"Caminho em ({x},{y}) fora da terra.");
    }

    // Caminhos só existem quando há pontes (múltiplas ilhas); numa grade toda-terra não há caminhos.
    [Fact]
    public void Generate_SingleLandmass_ProducesNoPaths()
    {
        // threshold muito baixo → tudo terra → 1 componente → EnsureConnected não desenha pontes
        var (_, isPath) = TerrainGenerator.Generate(30, 30, seed: 42, frequency: 0.05f, threshold: -0.99f);

        for (int x = 0; x <= 30; x++)
        for (int y = 0; y <= 30; y++)
            Assert.False(isPath[x, y], $"Sem ilhas isoladas não deve haver caminho em ({x},{y}).");
    }

    // Com arquipélago as pontes do MST devem gerar ao menos um componente de caminho.
    [Theory]
    [InlineData(7,   0.15f)]
    [InlineData(1,   0.12f)]
    [InlineData(123, 0.13f)]
    public void Generate_Archipelago_HasPaths(int seed, float freq)
    {
        var (_, isPath) = TerrainGenerator.Generate(99, 99, seed, freq);

        int pathCells = CountPathCells(isPath);
        Assert.True(pathCells > 0,
            $"Arquipélago (seed={seed}, freq={freq}) deve ter caminhos entre ilhas.");
    }

    // Todos os caminhos — pontes inter-ilhas + conexões internas — devem ser um único componente.
    [Theory]
    [InlineData(7,   0.15f)]
    [InlineData(1,   0.12f)]
    [InlineData(123, 0.13f)]
    [InlineData(999, 0.10f)]
    [InlineData(42,  0.08f)]
    public void Generate_PathCells_FormSingleConnectedComponent(int seed, float freq)
    {
        var (_, isPath) = TerrainGenerator.Generate(99, 99, seed, freq);

        int components = CountPathComponents(isPath);
        Assert.True(components <= 1,
            $"Caminhos devem ser todos conectados, mas foram encontrados {components} componentes (seed={seed}, freq={freq}).");
    }

    // Versão específica para arquipélago: caminhos existem E são um único componente.
    [Fact]
    public void Generate_Archipelago_PathsAreConnected()
    {
        var (_, isPath) = TerrainGenerator.Generate(99, 99, seed: 7, frequency: 0.15f);

        Assert.Equal(1, CountPathComponents(isPath));
    }

    // Grade maior com muitas ilhas — todos os caminhos ainda devem ser conectados.
    [Fact]
    public void Generate_LargeArchipelago_PathsAreConnected()
    {
        var (_, isPath) = TerrainGenerator.Generate(149, 149, seed: 999999, frequency: 0.12f);

        int components = CountPathComponents(isPath);
        Assert.True(components <= 1,
            $"Grade grande: caminhos devem ser conectados, mas foram encontrados {components} componentes.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int CountLandComponents(bool[,] grid)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);
        var visited = new bool[w, h];
        int components = 0;

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            if (!grid[x, y] || visited[x, y]) continue;
            components++;
            var q = new Queue<(int, int)>();
            q.Enqueue((x, y));
            visited[x, y] = true;
            while (q.Count > 0)
            {
                var (cx, cy) = q.Dequeue();
                (int, int)[] neighbors = [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)];
                foreach (var (nx, ny) in neighbors)
                    if ((uint)nx < (uint)w && (uint)ny < (uint)h && grid[nx, ny] && !visited[nx, ny])
                        { visited[nx, ny] = true; q.Enqueue((nx, ny)); }
            }
        }

        return components == 0 ? 0 : components;
    }

    private static readonly (int dx, int dy)[] CardinalDirs = [(-1,0),(1,0),(0,-1),(0,1)];

    private static int CountPathComponents(bool[,] isPath)
    {
        int w = isPath.GetLength(0), h = isPath.GetLength(1);
        var visited = new bool[w, h];
        int components = 0;

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            if (!isPath[x, y] || visited[x, y]) continue;
            components++;
            FloodFill(isPath, visited, x, y, w, h);
        }

        return components;
    }

    private static void FloodFill(bool[,] isPath, bool[,] visited, int startX, int startY, int w, int h)
    {
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            foreach (var (dx, dy) in CardinalDirs)
            {
                int nx = cx + dx, ny = cy + dy;
                if (nx >= 0 && nx < w && ny >= 0 && ny < h && isPath[nx, ny] && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }
    }

    private static int CountPathCells(bool[,] isPath)
    {
        int count = 0;
        for (int x = 0; x < isPath.GetLength(0); x++)
        for (int y = 0; y < isPath.GetLength(1); y++)
            if (isPath[x, y]) count++;
        return count;
    }

    private static int CountLand(bool[,] grid, int cols, int rows)
    {
        int count = 0;
        for (int x = 0; x <= cols; x++)
        for (int y = 0; y <= rows; y++)
            if (grid[x, y]) count++;
        return count;
    }

    private static int CountWater(bool[,] grid, int cols, int rows)
    {
        int count = 0;
        for (int x = 0; x <= cols; x++)
        for (int y = 0; y <= rows; y++)
            if (!grid[x, y]) count++;
        return count;
    }
}
