using Xunit;

namespace FractalRiver.Tests;

public class BiomeMapTests
{
    // ── Dimensões ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(10,  10)]
    [InlineData(100, 80)]
    [InlineData(1,   1)]
    public void Assign_GridSize_IsColsByRows(int cols, int rows)
    {
        var biomes = BiomeMap.Assign(cols, rows, seed: 0);

        Assert.Equal(cols, biomes.GetLength(0));
        Assert.Equal(rows, biomes.GetLength(1));
    }

    // ── Determinismo ──────────────────────────────────────────────────────────

    [Fact]
    public void Assign_SameSeed_ProducesSameGrid()
    {
        var a = BiomeMap.Assign(50, 50, seed: 42);
        var b = BiomeMap.Assign(50, 50, seed: 42);

        for (int col = 0; col < 50; col++)
        for (int row = 0; row < 50; row++)
            Assert.Equal(a[col, row], b[col, row]);
    }

    [Fact]
    public void Assign_DifferentSeeds_ProduceDifferentGrids()
    {
        var a = BiomeMap.Assign(50, 50, seed: 1);
        var b = BiomeMap.Assign(50, 50, seed: 2);

        int differences = 0;
        for (int col = 0; col < 50; col++)
        for (int row = 0; row < 50; row++)
            if (a[col, row] != b[col, row]) differences++;

        Assert.True(differences > 0, "Seeds diferentes devem gerar mapas de bioma diferentes.");
    }

    // ── Norte/Sul ─────────────────────────────────────────────────────────────

    [Fact]
    public void Assign_FarNorth_IsSand()
    {
        var biomes = BiomeMap.Assign(100, 100, seed: 42);

        // Primeira linha: northness=0.99, noise ≈ ±1 → mínimo ≈ 0.84 > 0.80 sempre Sand
        for (int col = 0; col < 100; col++)
            Assert.Equal(Biome.Sand, biomes[col, 0]);
    }

    [Fact]
    public void Assign_FarSouth_IsSnow()
    {
        var biomes = BiomeMap.Assign(100, 100, seed: 42);

        // Última linha: southness=0.99, noise ≈ ±1 → mínimo ≈ 0.84 > 0.80 sempre Snow
        int lastRow = 99;
        for (int col = 0; col < 100; col++)
            Assert.Equal(Biome.Snow, biomes[col, lastRow]);
    }

    [Fact]
    public void Assign_Majority_SouthIs_Snow()
    {
        int cols = 200, rows = 200;
        var biomes = BiomeMap.Assign(cols, rows, seed: 42);

        // Últimas 15% das linhas devem ter maioria esmagadora de Snow
        int snowStart = rows * 17 / 20; // 85%
        int snowCount = 0, total = 0;
        for (int col = 0; col < cols; col++)
        for (int row = snowStart; row < rows; row++)
        {
            if (biomes[col, row] == Biome.Snow) snowCount++;
            total++;
        }

        Assert.True((double)snowCount / total > 0.9,
            $"Extremo sul deve ter >90% Snow, mas teve {snowCount}/{total}.");
    }

    [Fact]
    public void Assign_Majority_NorthIs_Sand()
    {
        int cols = 200, rows = 200;
        var biomes = BiomeMap.Assign(cols, rows, seed: 42);

        // Primeiras 15% das linhas devem ter maioria esmagadora de Sand
        int sandEnd = rows * 3 / 20;
        int sandCount = 0, total = 0;
        for (int col = 0; col < cols; col++)
        for (int row = 0; row < sandEnd; row++)
        {
            if (biomes[col, row] == Biome.Sand) sandCount++;
            total++;
        }

        Assert.True((double)sandCount / total > 0.9,
            $"Extremo norte deve ter >90% Sand, mas teve {sandCount}/{total}.");
    }

    [Fact]
    public void Assign_BoundaryRow_NorthHasMixedBiomes()
    {
        int cols = 200, rows = 200;
        var biomes = BiomeMap.Assign(cols, rows, seed: 42);

        // Na faixa do boundary norte (±5% ao redor do threshold), deve haver os dois biomas
        int boundaryStart = rows * 15 / 100;
        int boundaryEnd   = rows * 25 / 100;

        bool hasSand = false, hasGrass = false;
        for (int col = 0; col < cols; col++)
        for (int row = boundaryStart; row < boundaryEnd; row++)
        {
            if (biomes[col, row] == Biome.Sand)  hasSand  = true;
            if (biomes[col, row] == Biome.Grass) hasGrass = true;
        }

        Assert.True(hasSand && hasGrass,
            "Faixa de boundary norte deve conter Sand e Grass (fronteira orgânica).");
    }

    // ── Organicidade ──────────────────────────────────────────────────────────

    [Fact]
    public void Assign_BoundaryRow_HasMixedBiomes()
    {
        int cols = 200, rows = 200;
        var biomes = BiomeMap.Assign(cols, rows, seed: 42);

        // Na faixa do boundary (±5% ao redor do threshold), deve haver os dois biomas
        int boundaryStart = rows * 75 / 100;
        int boundaryEnd   = rows * 85 / 100;

        bool hasGrass = false, hasSnow = false;
        for (int col = 0; col < cols; col++)
        for (int row = boundaryStart; row < boundaryEnd; row++)
        {
            if (biomes[col, row] == Biome.Grass) hasGrass = true;
            if (biomes[col, row] == Biome.Snow)  hasSnow  = true;
        }

        Assert.True(hasGrass && hasSnow,
            "Faixa de boundary deve conter ambos Grass e Snow (fronteira orgânica).");
    }
}
