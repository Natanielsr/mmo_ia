using GameServerApp.Services.World.MapGenerator.Generation;

namespace GameServerApp.Services.World.MapGenerator;

public static class TerrainGenerator
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

    private static void ForceBorderTiles(TileType[,] tiles, int cols, int rows)
    {
        for (int x = 0; x < cols; x++)
        {
            tiles[x, 0]        = TileType.WaterFull;
            tiles[x, rows - 1] = TileType.WaterFull;
        }
        for (int y = 0; y < rows; y++)
        {
            tiles[0, y]        = TileType.WaterFull;
            tiles[cols - 1, y] = TileType.WaterFull;
        }
    }

    /// <summary>
    /// Gera um mundo completo com tiles e biomas, pronto para renderizar ou serializar.
    /// </summary>
    public static WorldData GenerateWorld(int cols, int rows, int seed, float frequency, float threshold = -0.5f)
    {
        var (isLand, isPath) = Generate(cols, rows, seed, frequency, threshold);
        var tiles  = DualGrid.Compute(isLand, isPath);
        ForceBorderTiles(tiles, cols, rows);
        var biomes = BiomeMap.Assign(cols, rows, seed);
        return new WorldData(cols, rows, tiles, biomes);
    }
}
