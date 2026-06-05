using FractalRiver.Generation;

namespace FractalRiver;

public static class WorldGenerator
{
    /// <summary>
    /// Gera grades de terreno de tamanho (cols+1) × (rows+1) usando Perlin noise 2D.
    /// Retorna (isLand, isPath): isLand=true → grama/caminho; isPath=true → caminho (subconjunto de isLand).
    /// threshold=-0.5 → ~80% terra com linhas de rios. Frequências menores (0.02) geram
    /// continentes; maiores (0.1) geram arquipélagos.
    /// </summary>
    public static (bool[,] isLand, bool[,] isPath) Generate(int cols, int rows, int seed, float frequency, float threshold = -0.5f)
    {
        var (grid, paths)   = TerrainSampler.Sample(cols, rows, seed, frequency, threshold);
        var (labels, count) = ConnectivityAnalyzer.Label(grid);

        if (count > 1)
            IslandConnector.Connect(grid, paths, labels, count, seed);

        return (grid, paths);
    }

    /// <summary>
    /// Gera um mundo completo com tiles e biomas, pronto para renderizar ou serializar.
    /// </summary>
    public static WorldData GenerateWorld(int cols, int rows, int seed, float frequency, float threshold = -0.5f)
    {
        var (isLand, isPath) = Generate(cols, rows, seed, frequency, threshold);
        var tiles  = DualGrid.Compute(isLand, isPath);
        var biomes = BiomeMap.Assign(cols, rows, seed);
        return new WorldData(cols, rows, tiles, biomes);
    }
}
