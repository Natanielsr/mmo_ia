using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services.Ranking;

public interface IRankingService
{
    void RecordDeath(IPlayer player);
    IReadOnlyList<RankingEntryData> GetTopRanking(int count = 10);
}
