namespace GameServerApp.Services.World.MapGenerator.Generation;

internal static class TerrainSampler
{
    internal static (bool[,] grid, bool[,] paths) Sample(int cols, int rows, int seed, float frequency, float threshold)
    {
        var terrainNoise = new PerlinNoise2D(seed);
        var riverNoise   = new PerlinNoise2D(seed + 1000);
        var grid         = new bool[cols + 1, rows + 1];
        var paths        = new bool[cols + 1, rows + 1];

        float riverFreq  = frequency * 0.6f;
        float riverWidth = 0.06f;

        for (int x = 0; x <= cols; x++)
        for (int y = 0; y <= rows; y++)
        {
            bool isLand  = terrainNoise.Sample(x * frequency, y * frequency) >= threshold;
            bool isRiver = threshold > -0.9f && MathF.Abs(riverNoise.Sample(x * riverFreq, y * riverFreq)) < riverWidth;
            grid[x, y]   = isLand && !isRiver;
        }

        return (grid, paths);
    }
}
