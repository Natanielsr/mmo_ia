namespace GameServerApp.Contracts
{
    public interface IConnectionService
    {
        string Connect(string sessionToken);
        bool Disconnect(string connectionId);
        bool ValidateConnection(string connectionId);
        bool Reconnect(string connectionId);
    }
}