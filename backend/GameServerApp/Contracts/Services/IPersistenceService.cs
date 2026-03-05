using GameServerApp.Contracts.World;
namespace GameServerApp.Contracts.Services
{
    public interface IPersistenceService
    {
        void SavePlayerState(IPlayer player);
        IPlayer LoadPlayerState(string playerId);
    }
}