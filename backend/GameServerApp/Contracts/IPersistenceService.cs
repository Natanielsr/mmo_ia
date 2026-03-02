namespace GameServerApp.Contracts
{
    public interface IPersistenceService
    {
        void SavePlayerState(IPlayer player);
        IPlayer LoadPlayerState(string playerId);
    }
}