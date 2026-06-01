using GameServerApp.Dtos;
using GameServerApp.Repositories;

namespace GameServer.Tests.DB;

public class JsonRankingRepositoryTests : IDisposable
{
    private readonly string _tempFile;

    public JsonRankingRepositoryTests()
    {
        _tempFile = Path.GetTempFileName();
        File.WriteAllText(_tempFile, "[]");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void Save_And_Load_RoundTrip()
    {
        var repo = new JsonRankingRepository(_tempFile);
        var entries = new List<RankingEntryData>
        {
            new RankingEntryData
            {
                Name = "Hero",
                Level = 10,
                TotalExperience = 9000,
                JoinedAt = "2026-01-01T10:00:00Z",
                DiedAt = "2026-01-01T11:00:00Z",
                TimeSurvivedSeconds = 3600
            }
        };

        repo.Save(entries);
        var loaded = repo.Load();

        Assert.Single(loaded);
        Assert.Equal("Hero", loaded[0].Name);
        Assert.Equal(10, loaded[0].Level);
        Assert.Equal(9000, loaded[0].TotalExperience);
        Assert.Equal(3600, loaded[0].TimeSurvivedSeconds);
    }

    [Fact]
    public void Load_ReturnsEmpty_WhenFileNotFound()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");
        var repo = new JsonRankingRepository(missingPath);

        var result = repo.Load();

        Assert.Empty(result);
    }

    [Fact]
    public void Save_MultipleTimes_OverwritesPreviousData()
    {
        var repo = new JsonRankingRepository(_tempFile);

        repo.Save(new List<RankingEntryData> {
            new RankingEntryData { Name = "First", Level = 1 }
        });
        repo.Save(new List<RankingEntryData> {
            new RankingEntryData { Name = "Second", Level = 2 },
            new RankingEntryData { Name = "Third", Level = 3 }
        });

        var loaded = repo.Load();
        Assert.Equal(2, loaded.Count);
        Assert.Equal("Second", loaded[0].Name);
    }
}
