using System.Collections.Concurrent;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;

namespace GameServerApp.Managers;

public class PlayerManager : IPlayerManager
{
    private readonly ConcurrentDictionary<string, IPlayer> _playersByConnection = new();
    private readonly ConcurrentDictionary<long, IPlayer> _playersById = new();

    public void AddPlayer(string connectionId, IPlayer player)
    {
        _playersByConnection[connectionId] = player;
        _playersById[player.Id] = player;
    }

    public bool RemovePlayer(string connectionId, out IPlayer? player)
    {
        if (_playersByConnection.TryRemove(connectionId, out player))
        {
            _playersById.TryRemove(player.Id, out _);
            return true;
        }
        return false;
    }

    public IPlayer? GetPlayerByConnectionId(string connectionId)
    {
        return _playersByConnection.TryGetValue(connectionId, out var player) ? player : null;
    }

    public IPlayer? GetPlayerById(long id)
    {
        return _playersById.TryGetValue(id, out var player) ? player : null;
    }

    public IEnumerable<IPlayer> GetAllPlayers()
    {
        return _playersByConnection.Values;
    }
}
