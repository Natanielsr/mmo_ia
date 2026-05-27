namespace GameServerApp.Dtos;

public record ItemDefinition
{
    public required int Id { get; init; }
    public required string TagName { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int Value { get; init; }
    public required string Type { get; init; }
    public required float Weight { get; init; }
    public int? AttackBonus { get; init; }
    public int? DefenseBonus { get; init; }
    public int? HealAmount { get; init; }
}
