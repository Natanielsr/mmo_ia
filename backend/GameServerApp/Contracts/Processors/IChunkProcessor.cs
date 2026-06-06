using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Processors
{
    public interface IChunkProcessor
    {
        void ProcessChunkLoading(IPlayer player, string connectionId);
        void ProcessChunkRequest(IPlayer player, string connectionId, IEnumerable<ChunkCoordDto> coords);
    }
}
