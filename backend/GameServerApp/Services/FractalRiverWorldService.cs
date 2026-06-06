using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services;

public class FractalRiverWorldService : IFractalRiverWorldService
{
    private static readonly HashSet<TileType> PathTiles = new()
    {
        TileType.PathFull,
        TileType.TerrainPathCornerNW,
        TileType.TerrainPathCornerNE,
        TileType.TerrainPathCornerSW,
        TileType.TerrainPathCornerSE,
        TileType.PathTerrainEdgeN,
        TileType.PathTerrainEdgeS,
        TileType.PathTerrainEdgeE,
        TileType.PathTerrainEdgeW,
        TileType.PathTerrainCornerNW,
        TileType.PathTerrainCornerNE,
        TileType.PathTerrainCornerSW,
        TileType.PathTerrainCornerSE,
        TileType.PathTerrainCrossNWtoSE,
        TileType.PathTerrainCrossNEtoSW,
    };

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
        WorldData = FractalRiver.TerrainGenerator.GenerateWorld(map.Width, map.Height, map.GeneratorSeed, map.GeneratorFrequency);
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

    public bool IsPathTile(int worldX, int worldY)
    {
        if (worldX < 0 || worldX >= WorldData.Cols || worldY < 0 || worldY >= WorldData.Rows)
            return false;
        return PathTiles.Contains(WorldData.Tiles[worldX, worldY]);
    }

    public WorldMapData GetWorldMap() => new()
    {
        Cols = WorldData.Cols,
        Rows = WorldData.Rows,
        Tiles = WorldData.TilesAsJaggedArray(),
        Biomes = WorldData.BiomesAsJaggedArray(),
    };

    public (int[][] Tiles, int[][] Biomes) GetChunkTiles(int cx, int cy, int chunkSize)
    {
        var tiles  = new int[chunkSize][];
        var biomes = new int[chunkSize][];
        int startX = cx * chunkSize;
        int startY = cy * chunkSize;

        for (int row = 0; row < chunkSize; row++)
        {
            tiles[row]  = new int[chunkSize];
            biomes[row] = new int[chunkSize];
            for (int col = 0; col < chunkSize; col++)
            {
                int wx = startX + col;
                int wy = startY + row;
                tiles[row][col]  = (wx < WorldData.Cols && wy < WorldData.Rows) ? (int)WorldData.Tiles[wx, wy]  : 0;
                biomes[row][col] = (wx < WorldData.Cols && wy < WorldData.Rows) ? (int)WorldData.Biomes[wx, wy] : 0;
            }
        }

        return (tiles, biomes);
    }
}
