using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services.World;

public class WorldBiomeMapService : IBiomeMap
{
    private readonly Biome[,] _biomes;

    public WorldBiomeMapService(IOptions<WorldConfig> config)
    {
        var map  = config.Value.Map;
        _biomes  = BiomeMap.Assign(map.Width, map.Height, map.GeneratorSeed);
    }

    public Biome GetBiomeAt(int tileX, int tileY)
    {
        int col = Math.Clamp(tileX, 0, _biomes.GetLength(0) - 1);
        int row = Math.Clamp(tileY, 0, _biomes.GetLength(1) - 1);
        return _biomes[col, row];
    }
}
