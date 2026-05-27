using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Dtos;

public record class ItemData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = "";
    public int Value { get; init; } = 0;
    public string TagName { get; init; } = "";
    public required Position Position { get; init; }
    public required ItemType Type { get; init; }
    public int? AttackBonus { get; init; }
    public int? DefenseBonus { get; init; }
    public int SlotIndex { get; init; } = 0;
    public int Quantity { get; init; } = 1;
}
