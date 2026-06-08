using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services.World;

public interface IBiomeSelector
{
    BiomeDefinition GetBiomeForChunk(ChunkCoord coord);
    BiomeDefinition GetBiomeForPosition(Position pos);
    BiomeDefinition GetBiomeForTile(int tileX, int tileY);
}
