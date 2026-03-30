using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerStatusData
{
    public required string Id { get; init; }
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public bool IsDead { get; init; }
    public int Level { get; init; }
    public long Experience { get; init; }
}
