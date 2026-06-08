using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IWorldTerrainService
{
    FractalRiver.WorldData WorldData { get; }

    int? GetTileType(int worldX, int worldY);

    bool IsWaterTile(int worldX, int worldY);

    bool IsPathTile(int worldX, int worldY);

    /// <summary>Matriz completa do mundo (tiles + biomas) para enviar ao frontend.</summary>
    WorldMapData GetWorldMap();

    /// <summary>Extrai tiles e biomas de um chunk específico (chunkSize×chunkSize, convenção [row][col]).</summary>
    (int[][] Tiles, int[][] Biomes) GetChunkTiles(int cx, int cy, int chunkSize);
}
