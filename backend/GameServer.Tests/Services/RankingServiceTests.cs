using Moq;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Repositories;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Services;

namespace GameServer.Tests.Services;

public class RankingServiceTests
{
    private Mock<IRankingRepository> MockRepo() => new Mock<IRankingRepository>();

    private Mock<IPlayer> MakePlayer(string name, int level, long xp, DateTime joinedAt)
    {
        var p = new Mock<IPlayer>();
        p.Setup(x => x.Name).Returns(name);
        p.Setup(x => x.Level).Returns(level);
        p.Setup(x => x.Experience).Returns(xp);
        p.Setup(x => x.JoinedAt).Returns(joinedAt);
        return p;
    }

    [Fact]
    public void RecordDeath_AddsEntry()
    {
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(new List<RankingEntryData>());
        var svc = new RankingService(repo.Object);

        var player = MakePlayer("Hero", 5, 4000, DateTime.UtcNow.AddMinutes(-10));
        svc.RecordDeath(player.Object);

        var top = svc.GetTopRanking();
        Assert.Single(top);
        Assert.Equal("Hero", top[0].Name);
        Assert.Equal(5, top[0].Level);
        Assert.Equal(4000, top[0].TotalExperience);
    }

    [Fact]
    public void GetTopRanking_SortsByLevelDesc()
    {
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(new List<RankingEntryData>());
        var svc = new RankingService(repo.Object);

        svc.RecordDeath(MakePlayer("Low", 2, 1000, DateTime.UtcNow.AddMinutes(-5)).Object);
        svc.RecordDeath(MakePlayer("High", 10, 9000, DateTime.UtcNow.AddMinutes(-30)).Object);
        svc.RecordDeath(MakePlayer("Mid", 5, 4000, DateTime.UtcNow.AddMinutes(-15)).Object);

        var top = svc.GetTopRanking();
        Assert.Equal("High", top[0].Name);
        Assert.Equal("Mid", top[1].Name);
        Assert.Equal("Low", top[2].Name);
    }

    [Fact]
    public void GetTopRanking_TiesResolvedByTimeSurvivedDesc()
    {
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(new List<RankingEntryData>());
        var svc = new RankingService(repo.Object);

        var now = DateTime.UtcNow;
        svc.RecordDeath(MakePlayer("Short", 5, 4000, now.AddSeconds(-10)).Object);
        svc.RecordDeath(MakePlayer("Long", 5, 4500, now.AddMinutes(-60)).Object);

        var top = svc.GetTopRanking();
        Assert.Equal("Long", top[0].Name);
        Assert.Equal("Short", top[1].Name);
    }

    [Fact]
    public void GetTopRanking_RespectsCount()
    {
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(new List<RankingEntryData>());
        var svc = new RankingService(repo.Object);

        for (int i = 1; i <= 5; i++)
            svc.RecordDeath(MakePlayer($"Player{i}", i, i * 1000L, DateTime.UtcNow.AddMinutes(-i)).Object);

        var top = svc.GetTopRanking(3);
        Assert.Equal(3, top.Count);
    }

    [Fact]
    public void RecordDeath_PersistsViaRepository()
    {
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(new List<RankingEntryData>());
        var svc = new RankingService(repo.Object);

        svc.RecordDeath(MakePlayer("Hero", 3, 2000, DateTime.UtcNow.AddMinutes(-5)).Object);

        repo.Verify(r => r.Save(It.IsAny<IList<RankingEntryData>>()), Times.Once);
    }

    [Fact]
    public void RankingService_LoadsExistingEntriesFromRepository()
    {
        var existing = new List<RankingEntryData>
        {
            new RankingEntryData { Name = "Veteran", Level = 15, TotalExperience = 14000,
                JoinedAt = DateTime.UtcNow.AddHours(-2).ToString("o"),
                DiedAt = DateTime.UtcNow.AddHours(-1).ToString("o"),
                TimeSurvivedSeconds = 3600 }
        };
        var repo = MockRepo();
        repo.Setup(r => r.Load()).Returns(existing);

        var svc = new RankingService(repo.Object);
        var top = svc.GetTopRanking();

        Assert.Single(top);
        Assert.Equal("Veteran", top[0].Name);
    }
}
