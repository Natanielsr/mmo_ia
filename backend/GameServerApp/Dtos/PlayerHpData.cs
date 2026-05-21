namespace GameServerApp.Dtos;

public record class PlayerHpData
{
    public required string Id { get; init; }
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public bool IsDead { get; init; }
}
