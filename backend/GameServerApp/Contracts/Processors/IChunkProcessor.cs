using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Processors
{
    public interface IChunkProcessor
    {
        void ProcessChunkLoading(IPlayer player, string connectionId);
    }
}
