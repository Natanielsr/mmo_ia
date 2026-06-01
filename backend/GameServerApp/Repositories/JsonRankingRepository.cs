using System.Text.Json;
using GameServerApp.Contracts.Repositories;
using GameServerApp.Dtos;

namespace GameServerApp.Repositories;

public class JsonRankingRepository : IRankingRepository
{
    private readonly string _filePath;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public JsonRankingRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IList<RankingEntryData> Load()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
                return new List<RankingEntryData>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RankingEntryData>>(json, _jsonOptions)
                   ?? new List<RankingEntryData>();
        }
    }

    public void Save(IList<RankingEntryData> entries)
    {
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(entries, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
