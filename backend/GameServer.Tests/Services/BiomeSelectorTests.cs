using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GameServer.Tests.Services;

public class BiomeSelectorTests
{
    private readonly IBiomeSelector _sut;

    public BiomeSelectorTests()
    {
        var json = """
            {
              "biomes": [
                {
                  "id": 1,
                  "tagName": "green_field",
                  "name": "Green Field",
                  "groundTilePrefix": "grass",
                  "groundTileCount": 12,
                  "semanticObjectMap": { "default": "tree" },
                  "monsterTags": ["rat", "wolf"],
                  "isDefault": true
                },
                {
                  "id": 2,
                  "tagName": "dark_forest",
                  "name": "Dark Forest",
                  "groundTilePrefix": "darkgrass",
                  "groundTileCount": 12,
                  "semanticObjectMap": { "default": "dark_tree" },
                  "monsterTags": ["spider", "orc"],
                  "isDefault": false
                }
              ]
            }
            """;

        var repo = new BiomeDefinitionRepository(json);
        var config = Options.Create(new WorldConfig { Map = { ChunkSize = 16 } });
        _sut = new BiomeSelector(repo, config);
    }

    [Fact]
    public void Chunk_At_Origin_Returns_Default_Biome()
    {
        var biome = _sut.GetBiomeForChunk(new ChunkCoord(0, 0));
        Assert.Equal("green_field", biome.TagName);
    }

    [Fact]
    public void Chunk_Within_SafeSpawnRadius_Returns_Default_Biome()
    {
        // Distance = sqrt(1+1) ≈ 1.41, below SafeSpawnRadius of 2
        var biome = _sut.GetBiomeForChunk(new ChunkCoord(1, 1));
        Assert.Equal("green_field", biome.TagName);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, -1)]
    public void Chunks_Near_Origin_Always_Return_GreenField(int cx, int cy)
    {
        var biome = _sut.GetBiomeForChunk(new ChunkCoord(cx, cy));
        Assert.Equal("green_field", biome.TagName);
    }

    [Fact]
    public void GetBiomeForPosition_Converts_Position_To_ChunkCoord()
    {
        // Position (0,0) → chunk (0,0) → green_field
        var biome = _sut.GetBiomeForPosition(new Position(0, 0));
        Assert.Equal("green_field", biome.TagName);
    }

    [Fact]
    public void GetBiomeForPosition_Large_Negative_Coords_Returns_Valid_Biome()
    {
        // Should not throw even with large negative coords
        var biome = _sut.GetBiomeForPosition(new Position(-500, -500));
        Assert.NotNull(biome);
    }

    [Fact]
    public void Biome_Has_Non_Empty_MonsterTags()
    {
        var biome = _sut.GetBiomeForChunk(new ChunkCoord(0, 0));
        Assert.NotEmpty(biome.MonsterTags);
    }
}
