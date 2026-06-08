using GameServerApp.Services.Combat;
using GameServerApp.Services.Items;
using GameServerApp.Services.Movement;
using GameServerApp.Services.Ranking;
using GameServerApp.Services.Repositories;
using GameServerApp.Services.World;
using GameServerApp.Services.World.WorldFormations;
using Xunit;

namespace GameServer.Tests.Services;

public class BiomeDefinitionRepositoryTests
{
    private const string TestJson = """
        {
          "biomes": [
            {
              "id": 1,
              "tagName": "green_field",
              "name": "Green Field",
              "groundTilePrefix": "grass",
              "groundTileCount": 12,
              "objects": ["tree", "rock"],
              "monsterTags": ["rat", "wolf"],
              "isDefault": true
            },
            {
              "id": 2,
              "tagName": "dark_forest",
              "name": "Dark Forest",
              "groundTilePrefix": "darkgrass",
              "groundTileCount": 12,
              "objects": ["dark_tree", "dark_rock"],
              "monsterTags": ["spider", "orc"],
              "isDefault": false
            }
          ]
        }
        """;

    private readonly BiomeDefinitionRepository _sut = new(TestJson);

    [Fact]
    public void GetById_Returns_CorrectBiome()
    {
        var biome = _sut.GetById(1);

        Assert.NotNull(biome);
        Assert.Equal("green_field", biome.TagName);
        Assert.Equal("Green Field", biome.Name);
        Assert.Equal("grass", biome.GroundTilePrefix);
        Assert.Equal(12, biome.GroundTileCount);
    }

    [Fact]
    public void GetById_Returns_Null_For_Unknown_Id()
    {
        Assert.Null(_sut.GetById(999));
    }

    [Fact]
    public void GetByTagName_Returns_CorrectBiome()
    {
        var biome = _sut.GetByTagName("dark_forest");

        Assert.NotNull(biome);
        Assert.Equal(2, biome.Id);
        Assert.Equal("darkgrass", biome.GroundTilePrefix);
        Assert.Contains("spider", biome.MonsterTags);
    }

    [Fact]
    public void GetByTagName_Returns_Null_For_Unknown_Tag()
    {
        Assert.Null(_sut.GetByTagName("desert"));
    }

    [Fact]
    public void GetDefault_Returns_Biome_Marked_IsDefault()
    {
        var def = _sut.GetDefault();

        Assert.Equal("green_field", def.TagName);
        Assert.True(def.IsDefault);
    }

    [Fact]
    public void GetAll_Returns_All_Biomes()
    {
        Assert.Equal(2, _sut.GetAll().Count);
    }

    [Fact]
    public void Objects_Contains_Expected_Items()
    {
        var darkForest = _sut.GetByTagName("dark_forest")!;

        Assert.Contains("dark_tree", darkForest.Objects);
        Assert.Contains("dark_rock", darkForest.Objects);
    }

    [Fact]
    public void Constructor_Throws_If_Json_Invalid()
    {
        Assert.Throws<InvalidOperationException>(() => new BiomeDefinitionRepository("null"));
    }
}
