using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GameServer.Tests.Services;

public class BiomeSelectorTests
{
    private readonly Mock<IBiomeMap> _mockBiomeMap = new();
    private readonly IBiomeSelector _sut;

    public BiomeSelectorTests()
    {
        var json = """
            {
              "biomes": [
                {
                  "id": 1, "tagName": "green_field", "name": "Green Field",
                  "groundTilePrefix": "grass", "groundTileCount": 12,
                  "semanticObjectMap": { "default": "tree" },
                  "monsterTags": ["rat", "wolf"], "isDefault": true
                },
                {
                  "id": 2, "tagName": "dark_forest", "name": "Dark Forest",
                  "groundTilePrefix": "darkgrass", "groundTileCount": 12,
                  "semanticObjectMap": { "default": "dark_tree" },
                  "monsterTags": ["spider", "orc"], "isDefault": false
                },
                {
                  "id": 3, "tagName": "sand", "name": "Desert",
                  "groundTilePrefix": "sand", "groundTileCount": 12,
                  "semanticObjectMap": { "default": "cactus" },
                  "monsterTags": ["scorpion", "snake"], "isDefault": false
                },
                {
                  "id": 4, "tagName": "snow", "name": "Snowlands",
                  "groundTilePrefix": "snow", "groundTileCount": 12,
                  "semanticObjectMap": { "default": "pine_tree" },
                  "monsterTags": ["wolf", "bear"], "isDefault": false
                }
              ]
            }
            """;

        var repo   = new BiomeDefinitionRepository(json);
        var config = Options.Create(new WorldConfig { Map = { ChunkSize = 16 } });
        _sut = new BiomeSelector(repo, _mockBiomeMap.Object, config);
    }

    // --- Sand ---

    [Fact]
    public void GetBiomeForTile_WhenBiomeMapReturnsSand_ReturnsSandDefinition()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Sand);
        Assert.Equal("sand", _sut.GetBiomeForTile(0, -200).TagName);
    }

    [Fact]
    public void GetBiomeForTile_Sand_HasScorpionAndSnakeMonsterTags()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Sand);
        var biome = _sut.GetBiomeForTile(0, -200);
        Assert.Contains("scorpion", biome.MonsterTags);
        Assert.Contains("snake",    biome.MonsterTags);
    }

    // --- Snow ---

    [Fact]
    public void GetBiomeForTile_WhenBiomeMapReturnsSnow_ReturnsSnowDefinition()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Snow);
        Assert.Equal("snow", _sut.GetBiomeForTile(0, 200).TagName);
    }

    [Fact]
    public void GetBiomeForTile_Snow_HasWolfAndBearMonsterTags()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Snow);
        var biome = _sut.GetBiomeForTile(0, 200);
        Assert.Contains("wolf", biome.MonsterTags);
        Assert.Contains("bear", biome.MonsterTags);
    }

    // --- Grass ---

    [Fact]
    public void GetBiomeForTile_WhenBiomeMapReturnsGrass_ReturnsGreenField()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Grass);
        Assert.Equal("green_field", _sut.GetBiomeForTile(0, 0).TagName);
    }

    // --- DarkForest ---

    [Fact]
    public void GetBiomeForTile_WhenBiomeMapReturnsDarkForest_ReturnsDarkForestDefinition()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.DarkForest);
        Assert.Equal("dark_forest", _sut.GetBiomeForTile(100, 0).TagName);
    }

    [Fact]
    public void GetBiomeForTile_DarkForest_HasSpiderAndOrcMonsterTags()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.DarkForest);
        var biome = _sut.GetBiomeForTile(100, 0);
        Assert.Contains("spider", biome.MonsterTags);
        Assert.Contains("orc",    biome.MonsterTags);
    }

    // --- Delegação ---

    [Fact]
    public void GetBiomeForChunk_DelegatesToBiomeMapOnce()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Sand);
        var biome = _sut.GetBiomeForChunk(new ChunkCoord(10, -15));
        Assert.Equal("sand", biome.TagName);
        _mockBiomeMap.Verify(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void GetBiomeForPosition_DelegatesToBiomeMapOnce()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Snow);
        var biome = _sut.GetBiomeForPosition(new Position(100, 200));
        Assert.Equal("snow", biome.TagName);
        _mockBiomeMap.Verify(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void GetBiomeForTile_CallsBiomeMapWithExactCoordinates()
    {
        _mockBiomeMap.Setup(m => m.GetBiomeAt(42, -77)).Returns(Biome.Sand);
        _sut.GetBiomeForTile(42, -77);
        _mockBiomeMap.Verify(m => m.GetBiomeAt(42, -77), Times.Once);
    }

    // --- Fallback ---

    [Fact]
    public void GetBiomeForTile_WhenSandBiomeMissing_ReturnsDefault()
    {
        // Build selector with repo that has no "sand" entry → falls back to default
        var minimalJson = """
            {
              "biomes": [
                {
                  "id": 1, "tagName": "green_field", "name": "Green Field",
                  "groundTilePrefix": "grass", "groundTileCount": 12,
                  "semanticObjectMap": {}, "monsterTags": [], "isDefault": true
                }
              ]
            }
            """;
        var mockBiomeMap = new Mock<IBiomeMap>();
        mockBiomeMap.Setup(m => m.GetBiomeAt(It.IsAny<int>(), It.IsAny<int>())).Returns(Biome.Sand);
        var repo   = new BiomeDefinitionRepository(minimalJson);
        var config = Options.Create(new WorldConfig { Map = { ChunkSize = 16 } });
        var sut    = new BiomeSelector(repo, mockBiomeMap.Object, config);

        var biome = sut.GetBiomeForTile(0, 0);
        Assert.Equal("green_field", biome.TagName); // fallback to default
    }
}
