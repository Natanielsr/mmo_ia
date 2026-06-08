using GameServerApp.Services.World.MapGenerator;
using System.Reflection;
using Xunit;

namespace FractalRiver.Tests;

#if RENDERING_TESTS

// Valida que GrassTileSet.TileFiles e BiomeTileSet.SharedTileFiles cobrem todos os TileType.
// Acessa campos privados via reflexão para não alterar visibilidade do código de produção.
public class TileMapRendererTests
{
    private static Dictionary<TileType, string> GetGrassTileFiles()
    {
        var field = typeof(GrassTileSet).GetField(
            "TileFiles", BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<TileType, string>)field!.GetValue(null)!;
    }

    private static Dictionary<TileType, string> GetSharedTileFiles()
    {
        var field = typeof(BiomeTileSet).GetField(
            "SharedTileFiles", BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<TileType, string>)field!.GetValue(null)!;
    }

    [Theory]
    // tiles compartilhados (BiomeTileSet.SharedTileFiles)
    [InlineData(TileType.WaterFull,             "water_full.png")]
    // tiles do bioma grama (GrassTileSet.TileFiles)
    [InlineData(TileType.TerrainFull,           "grass_full.png")]
    [InlineData(TileType.TerrainWaterCornerNW,  "grass_water_corner_NW.png")]
    [InlineData(TileType.TerrainWaterCornerNE,  "grass_water_corner_NE.png")]
    [InlineData(TileType.TerrainWaterCornerSW,  "grass_water_corner_SW.png")]
    [InlineData(TileType.TerrainWaterCornerSE,  "grass_water_corner_SE.png")]
    [InlineData(TileType.WaterTerrainEdgeN,     "water_grass_edge_N.png")]
    [InlineData(TileType.WaterTerrainEdgeS,     "water_grass_edge_S.png")]
    [InlineData(TileType.WaterTerrainEdgeE,     "water_grass_edge_E.png")]
    [InlineData(TileType.WaterTerrainEdgeW,     "water_grass_edge_W.png")]
    [InlineData(TileType.WaterTerrainCornerNW,  "water_grass_corner_NW.png")]
    [InlineData(TileType.WaterTerrainCornerNE,  "water_grass_corner_NE.png")]
    [InlineData(TileType.WaterTerrainCornerSW,  "water_grass_corner_SW.png")]
    // duplo underscore intencional em water_grass__corner_SE.png
    [InlineData(TileType.WaterTerrainCornerSE,  "water_grass__corner_SE.png")]
    public void GrassTileFiles_TileType_MapeiaFilenameCorreto(TileType type, string expectedFilename)
    {
        var grass  = GetGrassTileFiles();
        var shared = GetSharedTileFiles();
        var combined = new Dictionary<TileType, string>(grass);
        foreach (var (k, v) in shared) combined[k] = v;

        Assert.True(combined.ContainsKey(type), $"Nenhum TileSet contém entrada para {type}");
        Assert.Equal(expectedFilename, combined[type]);
    }

    [Fact]
    public void GrassETileFilesCompartilhados_CobremTodosOsTileTypes()
    {
        var grass  = GetGrassTileFiles();
        var shared = GetSharedTileFiles();
        var combined = grass.Keys.Concat(shared.Keys).ToHashSet();

        foreach (var type in Enum.GetValues<TileType>())
            Assert.True(combined.Contains(type), $"Nenhum TileSet cobre {type}");
    }
}

#endif
