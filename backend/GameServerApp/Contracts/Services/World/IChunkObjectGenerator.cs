using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services.World
{
    public interface IChunkObjectGenerator
    {
        void GenerateChunk(ChunkCoord coord);
    }
}
