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

    // Multi-octave noise in [0,1] — produces organic patches of ~8-15 chunk diameter
    private static double GetNoise(int cx, int cy)
    {
        double val = Math.Sin(cx * 0.31) * Math.Cos(cy * 0.29)          // large patches
                   + Math.Sin(cx * 0.17 + cy * 0.13) * 0.5              // medium variation
                   + Math.Sin(cx * 0.07 - cy * 0.11) * 0.25;            // fine detail
        // val range ≈ [-1.75, 1.75] → normalize to [0,1]
        return (val + 1.75) / 3.5;
    }
}
