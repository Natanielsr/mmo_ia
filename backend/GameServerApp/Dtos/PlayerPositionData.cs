using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerPositionData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
}
