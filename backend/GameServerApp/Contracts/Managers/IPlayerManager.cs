using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Managers;

public interface IPlayerManager
{
    void AddPlayer(string connectionId, IPlayer player);
    bool RemovePlayer(string connectionId, out IPlayer? player);
    IPlayer? GetPlayerByConnectionId(string connectionId);
    IPlayer? GetPlayerById(long id);
    string? GetConnectionIdByPlayerId(long playerId);
    IEnumerable<IPlayer> GetAllPlayers();
}
