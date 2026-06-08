namespace GameServerApp.Services.World.MapGenerator.Generation;

internal static class PathRouter
{
    // Frequência de noise fina (~12 tiles) para criar curvas suaves sem micro-jitter
    private const float NoiseFreq = 0.08f;

    internal static void Connect(bool[,] grid, bool[,] paths, (int x, int y) from, (int x, int y) to, Func<float, float, float> sample, int halfWidth = 1)
    {
        if (from == to) return;

        int w       = grid.GetLength(0), h = grid.GetLength(1);
        var dist    = new Dictionary<(int, int), float>();
        var prev    = new Dictionary<(int x, int y), (int x, int y)>();
        var visited = new HashSet<(int, int)>();
        var pq      = new PriorityQueue<(int x, int y), float>();

        dist[from] = 0f;
        prev[from] = from;
        pq.Enqueue(from, 0f);

        while (pq.Count > 0)
        {
            var cur = pq.Dequeue();
            if (!visited.Add(cur)) continue;
            if (cur == to) break;

            float curCost = dist[cur];
            foreach (var next in GridUtilities.Adj4(cur.x, cur.y, w, h))
            {
                if (!grid[next.x, next.y] || visited.Contains(next)) continue;

                // Custo varia de 1.0 (barato) a 2.5 (caro) segundo o noise
                float n       = (sample(next.x * NoiseFreq, next.y * NoiseFreq) + 1f) * 0.5f;
                float newCost = curCost + 1f + n * 1.5f;

                if (!dist.TryGetValue(next, out float existing) || newCost < existing)
                {
                    dist[next] = newCost;
                    prev[next] = cur;
                    pq.Enqueue(next, newCost);
                }
            }
        }

        if (!prev.ContainsKey(to)) return;

        var cell = to;
        while (cell != from) { MarkWide(cell); cell = prev[cell]; }
        MarkWide(from);

        void MarkWide((int x, int y) c)
        {
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            for (int dy = -halfWidth; dy <= halfWidth; dy++)
            {
                int mx = c.x + dx, my = c.y + dy;
                if ((uint)mx < (uint)w && (uint)my < (uint)h && grid[mx, my])
                    paths[mx, my] = true;
            }
        }
    }
}
