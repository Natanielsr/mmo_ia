using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class MonsterData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ObjectCode { get; init; }
    public required Position Position { get; init; }
    public required int Hp { get; init; }
    public required int MaxHp { get; init; }
    public required int AttackPower { get; init; }
    public required bool IsDead { get; init; }
}