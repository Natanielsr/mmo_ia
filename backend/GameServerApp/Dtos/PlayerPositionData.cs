using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerPositionData
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
}
