using GameServerApp.Services.World.MapGenerator.Generation;

namespace GameServerApp.Services.World.MapGenerator;

public static class BiomeMap
{
    private const float Frequency          = 0.04f;
    private const float NoiseStrength      = 0.15f;
    private const float SnowThreshold      = 0.80f;
    private const float SandThreshold      = 0.80f;
    private const float DarkForestThreshold = 0.65f;
    private const float DarkForestNoiseStr  = 0.10f;

    public static Biome[,] Assign(int cols, int rows, int seed)
    {
        var noise  = new PerlinNoise2D(seed + 2000);
        var biomes = new Biome[cols, rows];

        for (int col = 0; col < cols; col++)
        for (int row = 0; row < rows; row++)
        {
            float northness = 1.0f - (float)row / rows;
            float southness = (float)row / rows;
            float eastness  = (float)col / cols;
            float noiseVal  = noise.Sample(col * Frequency, row * Frequency);

            biomes[col, row] =
                northness + NoiseStrength    * noiseVal >= SandThreshold         ? Biome.Sand
              : southness + NoiseStrength    * noiseVal >= SnowThreshold         ? Biome.Snow
              : eastness  + DarkForestNoiseStr * noiseVal >= DarkForestThreshold ? Biome.DarkForest
              : Biome.Grass;
        }

        return biomes;
    }
}
