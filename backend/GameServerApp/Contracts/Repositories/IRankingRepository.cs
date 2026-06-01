using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Repositories;

public interface IRankingRepository
{
    IList<RankingEntryData> Load();
    void Save(IList<RankingEntryData> entries);
}
