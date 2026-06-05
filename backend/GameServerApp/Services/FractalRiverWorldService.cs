using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services;

public class FractalRiverWorldService : IFractalRiverWorldService
{
    private static readonly HashSet<TileType> WaterTiles = new()
    {
        TileType.WaterFull,
        TileType.TerrainWaterCornerNW,
        TileType.TerrainWaterCornerNE,
        TileType.TerrainWaterCornerSW,
        TileType.TerrainWaterCornerSE,
        TileType.WaterTerrainEdgeN,
        TileType.WaterTerrainEdgeS,
        TileType.WaterTerrainEdgeE,
        TileType.WaterTerrainEdgeW,
        TileType.WaterTerrainCornerNW,
        TileType.WaterTerrainCornerNE,
        TileType.WaterTerrainCornerSW,
        TileType.WaterTerrainCornerSE,
    };

    public WorldData WorldData { get; }

    public FractalRiverWorldService(IOptions<WorldConfig> config)
    {
        var map = config.Value.Map;
        WorldData = FractalRiver.WorldGenerator.GenerateWorld(map.Width, map.Height, map.GeneratorSeed, map.GeneratorFrequency);
    }

    public int? GetTileType(int worldX, int worldY)
    {
        if (worldX < 0 || worldX >= WorldData.Cols || worldY < 0 || worldY >= WorldData.Rows)
            return null;
        return (int)WorldData.Tiles[worldX, worldY];
    }

    public bool IsWaterTile(int worldX, int worldY)
    {
        if (worldX < 0 || worldX >= WorldData.Cols || worldY < 0 || worldY >= WorldData.Rows)
            return false;
        return WaterTiles.Contains(WorldData.Tiles[worldX, worldY]);
    }

    public WorldMapData GetWorldMap() => new()
    {
        Cols = WorldData.Cols,
        Rows = WorldData.Rows,
        Tiles = WorldData.TilesAsJaggedArray(),
        Biomes = WorldData.BiomesAsJaggedArray(),
    };
}
