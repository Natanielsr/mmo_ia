using GameServerApp.Contracts.Repositories;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Services;

public class RankingService : IRankingService
{
    private readonly IRankingRepository _repository;
    private readonly List<RankingEntryData> _entries;
    private readonly object _lock = new();

    public RankingService(IRankingRepository repository)
    {
        _repository = repository;
        _entries = new List<RankingEntryData>(_repository.Load());
    }

    public void RecordDeath(IPlayer player)
    {
        var diedAt = DateTime.UtcNow;
        var survived = (int)(diedAt - player.JoinedAt).TotalSeconds;

        var entry = new RankingEntryData
        {
            Name                = player.Name,
            Level               = player.Level,
            TotalExperience     = player.Experience,
            JoinedAt            = player.JoinedAt.ToString("o"),
            DiedAt              = diedAt.ToString("o"),
            TimeSurvivedSeconds = Math.Max(0, survived),
        };

        lock (_lock)
        {
            _entries.Add(entry);
            _repository.Save(_entries);
        }
    }

    public IReadOnlyList<RankingEntryData> GetTopRanking(int count = 10)
    {
        lock (_lock)
        {
            return _entries
                .OrderByDescending(e => e.Level)
                .ThenByDescending(e => e.TimeSurvivedSeconds)
                .Take(count)
                .ToList();
        }
    }
}
