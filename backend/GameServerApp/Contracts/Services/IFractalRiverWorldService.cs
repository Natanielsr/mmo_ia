using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IFractalRiverWorldService
{
    FractalRiver.WorldData WorldData { get; }

    int? GetTileType(int worldX, int worldY);

    bool IsWaterTile(int worldX, int worldY);

    /// <summary>Matriz completa do mundo (tiles + biomas) para enviar ao frontend.</summary>
    WorldMapData GetWorldMap();
}
