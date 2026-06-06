using System.Collections.Generic;

namespace GameServerApp.Dtos;

public class BiomeDefinition
{
    public required int Id { get; init; }
    public required string TagName { get; init; }
    public required string Name { get; init; }
    public required string GroundTilePrefix { get; init; }
    public required int GroundTileCount { get; init; }
    public required List<string> Objects { get; init; }
    public required List<string> MonsterTags { get; init; }
    public bool IsDefault { get; init; }
}
