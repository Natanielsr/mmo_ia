using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
    public Dictionary<string, string?> EquippedItems { get; init; } = new();
}
