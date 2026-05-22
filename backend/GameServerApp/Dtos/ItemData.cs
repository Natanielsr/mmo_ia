using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Dtos;

public record class ItemData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
    public required ItemType Type { get; init; }
    public int? AttackBonus { get; init; }
    public int? DefenseBonus { get; init; }
    public int SlotIndex { get; init; } = 0;
    public int Quantity { get; init; } = 1;
}
