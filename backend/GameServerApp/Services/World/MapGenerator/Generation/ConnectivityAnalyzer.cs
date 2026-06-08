namespace GameServerApp.Services.World.MapGenerator.Generation;

internal static class ConnectivityAnalyzer
{
    internal static (int[,] labels, int count) Label(bool[,] grid)
    {
        int w      = grid.GetLength(0), h = grid.GetLength(1);
        var labels = new int[w, h];
        int count  = 0;

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            if (!grid[x, y] || labels[x, y] != 0) continue;

            int id = ++count;
            var q  = new Queue<(int, int)>();
            q.Enqueue((x, y));
            labels[x, y] = id;

            while (q.Count > 0)
            {
                var (cx, cy) = q.Dequeue();
                foreach (var (nx, ny) in GridUtilities.Adj4(cx, cy, w, h))
                    if (grid[nx, ny] && labels[nx, ny] == 0) { labels[nx, ny] = id; q.Enqueue((nx, ny)); }
            }
        }

        return (labels, count);
    }
}
