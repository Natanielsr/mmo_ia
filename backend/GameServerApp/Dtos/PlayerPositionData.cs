using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerPositionData
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public bool IsDead { get; init; }
}
