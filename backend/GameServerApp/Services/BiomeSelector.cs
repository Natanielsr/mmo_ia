using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services;

public class BiomeSelector : IBiomeSelector
{
    private readonly IBiomeDefinitionRepository _biomeRepo;
    private readonly int _chunkSize;

    // Keep spawn area always green_field (2 chunks radius)
    private const double SafeSpawnRadius = 2.0;

    public BiomeSelector(IBiomeDefinitionRepository biomeRepo, IOptions<WorldConfig> config)
    {
        _biomeRepo = biomeRepo;
        _chunkSize = config.Value.Map.ChunkSize;
    }

    public BiomeDefinition GetBiomeForChunk(ChunkCoord coord)
    {
        var defaultBiome = _biomeRepo.GetDefault();

        var distance = Math.Sqrt((double)coord.CX * coord.CX + (double)coord.CY * coord.CY);
        if (distance < SafeSpawnRadius)
            return defaultBiome;

        var noise = GetNoise(coord.CX, coord.CY);
        if (noise >= 0.5)
        {
            var darkForest = _biomeRepo.GetByTagName("dark_forest");
            if (darkForest != null)
                return darkForest;
        }

        return defaultBiome;
    }

    public BiomeDefinition GetBiomeForPosition(Position pos)
    {
        int cx = (int)Math.Floor((double)pos.X / _chunkSize);
        int cy = (int)Math.Floor((double)pos.Y / _chunkSize);
        return GetBiomeForChunk(new ChunkCoord(cx, cy));
    }

    public BiomeDefinition GetBiomeForTile(int tileX, int tileY)
    {
        var defaultBiome = _biomeRepo.GetDefault();

        // Safe radius in tile units: 2 chunks * chunkSize
        double distTiles = Math.Sqrt((double)tileX * tileX + (double)tileY * tileY);
        if (distTiles < SafeSpawnRadius * _chunkSize)
            return defaultBiome;

        double fx = (double)tileX / _chunkSize;
        double fy = (double)tileY / _chunkSize;
        double noise = GetNoise(fx, fy);

        if (noise >= 0.5)
        {
            var darkForest = _biomeRepo.GetByTagName("dark_forest");
            if (darkForest != null) return darkForest;
        }

        return defaultBiome;
    }

    // Domain-warped multi-octave noise → irregular organic biome shapes, hard threshold
    private static double GetNoise(double fx, double fy)
    {
        // Warp input coords to break regularity
        double wx = fx + 1.8 * Math.Sin(fx * 0.23 + fy * 0.17);
        double wy = fy + 1.8 * Math.Cos(fx * 0.19 - fy * 0.29);

        double val = Math.Sin(wx * 0.31) * Math.Cos(wy * 0.29)
                   + Math.Sin(wx * 0.17 + wy * 0.13) * 0.5
                   + Math.Sin(wx * 0.07 - wy * 0.11) * 0.25;
        return (val + 1.75) / 3.5;
    }
}
