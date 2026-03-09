using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Dtos;

public record class MapObjectData
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
    public required ObjectType Type { get; init; }
    public required bool IsPassable { get; init; }
}