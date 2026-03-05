using GameServerApp.Contracts.World;
namespace GameServerApp.Contracts.Services
{
    public interface IConnectionService
    {
        string? Connect(string sessionToken);
        bool Disconnect(string connectionId);
        bool ValidateConnection(string connectionId);
        bool Reconnect(string connectionId);
    }
}