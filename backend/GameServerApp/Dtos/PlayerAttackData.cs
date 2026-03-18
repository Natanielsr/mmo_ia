namespace GameServerApp.Dtos;

public record class PlayerAttackData
{
    public required string AttackerId { get; init; }
    public required string AttackerName { get; init; }
    public required string TargetId { get; init; }
    public required string TargetName { get; init; }
    public int Damage { get; init; }
}
