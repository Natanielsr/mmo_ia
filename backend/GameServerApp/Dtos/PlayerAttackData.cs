namespace GameServerApp.Dtos;

public record class PlayerAttackData
{
    public required long AttackerId { get; init; }
    public required string AttackerName { get; init; }
    public required long TargetId { get; init; }
    public required string TargetName { get; init; }
    public int Damage { get; init; }
}
