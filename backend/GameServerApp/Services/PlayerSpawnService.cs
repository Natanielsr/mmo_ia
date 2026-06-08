using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using Microsoft.Extensions.Options;

namespace GameServerApp.Services;

public class PlayerSpawnService : IPlayerSpawnService
{
    private readonly IWorldTerrainService _fractalWorld;
    private readonly IStaticWorldManager _staticWorldManager;
    private readonly IChunkObjectGenerator _chunkGenerator;
    private readonly MapConfig _map;

    public PlayerSpawnService(
        IWorldTerrainService fractalWorld,
        IStaticWorldManager staticWorldManager,
        IChunkObjectGenerator chunkGenerator,
        IOptions<WorldConfig> config)
    {
        _fractalWorld = fractalWorld;
        _staticWorldManager = staticWorldManager;
        _chunkGenerator = chunkGenerator;
        _map = config.Value.Map;
    }

    public Position FindSafePosition()
    {
        int cols = _fractalWorld.WorldData.Cols;
        int rows = _fractalWorld.WorldData.Rows;
        int cx = cols / 2, cy = rows / 2;
        int maxR = Math.Max(cols, rows);

        for (int r = 0; r <= maxR; r++)
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                    int x = cx + dx, y = cy + dy;
                    if (x < 0 || y < 0 || x >= cols || y >= rows) continue;
                    if (_fractalWorld.IsWaterTile(x, y)) continue;

                    EnsureChunkLoaded(x, y);

                    if (_staticWorldManager.IsBlocked(new Position(x, y))) continue;
                    if (IsSurrounded(x, y, cols, rows)) continue;

                    return new Position(x, y);
                }

        return new Position(cx, cy);
    }

    private void EnsureChunkLoaded(int x, int y)
    {
        var coord = new ChunkCoord(x / _map.ChunkSize, y / _map.ChunkSize);
        if (!_staticWorldManager.IsChunkLoaded(coord))
            _chunkGenerator.GenerateChunk(coord);
    }

    private bool IsSurrounded(int x, int y, int cols, int rows)
    {
        (int dx, int dy)[] dirs = [(0, 1), (0, -1), (1, 0), (-1, 0)];
        foreach (var (dx, dy) in dirs)
        {
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= cols || ny >= rows) continue;
            if (_fractalWorld.IsWaterTile(nx, ny)) continue;
            EnsureChunkLoaded(nx, ny);
            if (!_staticWorldManager.IsBlocked(new Position(nx, ny)))
                return false;
        }
        return true;
    }
}
