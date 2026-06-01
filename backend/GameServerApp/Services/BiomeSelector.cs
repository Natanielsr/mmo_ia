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

        // Clear zones: no blending needed
        if (noise < 0.4) return defaultBiome;

        if (noise > 0.6)
        {
            var darkForest = _biomeRepo.GetByTagName("dark_forest");
            if (darkForest != null) return darkForest;
            return defaultBiome;
        }

        // Transition zone 0.4–0.6: deterministic per-tile blend matching frontend hash
        uint hash = (uint)(tileX * 2654435761 + tileY * 2246822519);
        double tileRng = hash / (double)uint.MaxValue;
        double prob = (noise - 0.4) / 0.2; // 0.0 at edge of green, 1.0 at edge of dark

        if (tileRng < prob)
        {
            var darkForest = _biomeRepo.GetByTagName("dark_forest");
            if (darkForest != null) return darkForest;
        }

        return defaultBiome;
    }

    // Multi-octave noise in [0,1] — accepts float chunk coords for per-tile resolution
    private static double GetNoise(double fx, double fy)
    {
        double val = Math.Sin(fx * 0.31) * Math.Cos(fy * 0.29)
                   + Math.Sin(fx * 0.17 + fy * 0.13) * 0.5
                   + Math.Sin(fx * 0.07 - fy * 0.11) * 0.25;
        return (val + 1.75) / 3.5;
    }
}
