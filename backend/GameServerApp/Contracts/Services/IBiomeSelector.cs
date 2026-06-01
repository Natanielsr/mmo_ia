using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IBiomeSelector
{
    BiomeDefinition GetBiomeForChunk(ChunkCoord coord);
    BiomeDefinition GetBiomeForPosition(Position pos);
}
