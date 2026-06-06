using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services
{
    public interface IChunkObjectGenerator
    {
        void GenerateChunk(ChunkCoord coord);
    }
}
