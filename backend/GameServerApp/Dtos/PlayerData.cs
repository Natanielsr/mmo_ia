using GameServerApp.Contracts.Types;

namespace GameServerApp.Dtos;

public record class PlayerData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Position Position { get; init; }
    public string? EquippedWeaponName { get; init; }
    public string? EquippedArmorName { get; init; }
    public string? EquippedLegsName { get; init; }
    public string? EquippedHelmetName { get; init; }
}
