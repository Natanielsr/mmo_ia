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

    [Fact]
    public void GetBiomeForTile_Within_SafeRadius_Returns_GreenField()
    {
        // Tiles within 2*chunkSize (32) from origin are always green_field
        var biome = _sut.GetBiomeForTile(10, 10);
        Assert.Equal("green_field", biome.TagName);
    }

    [Fact]
    public void GetBiomeForTile_At_Origin_Returns_GreenField()
    {
        var biome = _sut.GetBiomeForTile(0, 0);
        Assert.Equal("green_field", biome.TagName);
    }

    [Theory]
    [InlineData(100, 0)]
    [InlineData(200, 150)]
    [InlineData(-100, -100)]
    public void GetBiomeForTile_Far_From_Origin_Returns_Valid_Biome(int tx, int ty)
    {
        var biome = _sut.GetBiomeForTile(tx, ty);
        Assert.NotNull(biome);
        Assert.Contains(biome.TagName, new[] { "green_field", "dark_forest" });
    }

    [Fact]
    public void GetBiomeForTile_Is_Deterministic()
    {
        var b1 = _sut.GetBiomeForTile(150, 200);
        var b2 = _sut.GetBiomeForTile(150, 200);
        Assert.Equal(b1.TagName, b2.TagName);
    }

    [Fact]
    public void GetBiomeForTile_And_GetBiomeForChunk_Agree_On_Clear_Cases()
    {
        // Tile at (0,0) chunk center (tile 8,8) — safe radius, both green_field
        var tile = _sut.GetBiomeForTile(8, 8);
        var chunk = _sut.GetBiomeForChunk(new ChunkCoord(0, 0));
        Assert.Equal(chunk.TagName, tile.TagName);
    }
}
