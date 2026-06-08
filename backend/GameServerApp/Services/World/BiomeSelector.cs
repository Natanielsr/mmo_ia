using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services.World;

public class BiomeSelector : IBiomeSelector
{
    private readonly IBiomeDefinitionRepository _biomeRepo;
    private readonly IBiomeMap                  _biomeMap;
    private readonly int                        _chunkSize;

    public BiomeSelector(IBiomeDefinitionRepository biomeRepo, IBiomeMap biomeMap, IOptions<WorldConfig> config)
    {
        _biomeRepo = biomeRepo;
        _biomeMap  = biomeMap;
        _chunkSize = config.Value.Map.ChunkSize;
    }

    public BiomeDefinition GetBiomeForChunk(ChunkCoord coord)
    {
        int tileX = coord.CX * _chunkSize + _chunkSize / 2;
        int tileY = coord.CY * _chunkSize + _chunkSize / 2;
        return GetBiomeForTile(tileX, tileY);
    }

    public BiomeDefinition GetBiomeForPosition(Position pos)
    {
        return GetBiomeForTile(pos.X, pos.Y);
    }

    public BiomeDefinition GetBiomeForTile(int tileX, int tileY)
    {
        return _biomeMap.GetBiomeAt(tileX, tileY) switch
        {
            Biome.Sand       => _biomeRepo.GetByTagName("sand")        ?? _biomeRepo.GetDefault(),
            Biome.Snow       => _biomeRepo.GetByTagName("snow")        ?? _biomeRepo.GetDefault(),
            Biome.DarkForest => _biomeRepo.GetByTagName("dark_forest") ?? _biomeRepo.GetDefault(),
            _                => _biomeRepo.GetDefault()
        };
    }
}
