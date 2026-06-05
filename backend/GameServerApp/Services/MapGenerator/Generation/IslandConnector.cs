namespace FractalRiver.Generation;

internal static class IslandConnector
{
    internal static void Connect(bool[,] grid, bool[,] paths, int[,] labels, int count, int seed)
    {
        int w   = grid.GetLength(0), h = grid.GetLength(1);
        var rng = new Random(seed + 3000);
        var pathNoise = new PerlinNoise2D(seed + 4000);

        var borders = CollectBorders(grid, labels, count, w, h);
        var edges   = FindClosestPairs(borders, count);

        var parent = Enumerable.Range(0, count + 1).ToArray();
        int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }

        var endpointsByIsland = new Dictionary<int, List<(int x, int y)>>();

        foreach (var (_, a, b, pa, pb) in edges)
        {
            if (Find(a) == Find(b)) continue;
            parent[Find(a)] = Find(b);
            BridgeDrawer.Draw(grid, paths, pa, pb, rng);

            if (!endpointsByIsland.TryGetValue(a, out var la)) endpointsByIsland[a] = la = [];
            if (!endpointsByIsland.TryGetValue(b, out var lb)) endpointsByIsland[b] = lb = [];
            la.Add(pa);
            lb.Add(pb);
        }

        foreach (var endpoints in endpointsByIsland.Values)
            for (int i = 1; i < endpoints.Count; i++)
                PathRouter.Connect(grid, paths, endpoints[i - 1], endpoints[i], pathNoise.Sample);

        // Garante que todos os path segments formam 1 componente conectado
        EnsurePathConnectivity(grid, paths, rng, w, h);
    }

    private static void EnsurePathConnectivity(bool[,] grid, bool[,] paths, Random rng, int w, int h)
    {
        // BFS para contar componentes de path
        var visited = new bool[w, h];
        var components = new List<List<(int x, int y)>>();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (!paths[x, y] || visited[x, y]) continue;

                var comp = new List<(int x, int y)>();
                var q = new Queue<(int, int)>();
                q.Enqueue((x, y));
                visited[x, y] = true;

                while (q.Count > 0)
                {
                    var (cx, cy) = q.Dequeue();
                    comp.Add((cx, cy));
                    foreach (var (nx, ny) in GridUtilities.Adj4(cx, cy, w, h))
                    {
                        if (paths[nx, ny] && !visited[nx, ny])
                        {
                            visited[nx, ny] = true;
                            q.Enqueue((nx, ny));
                        }
                    }
                }

                components.Add(comp);
            }
        }

        // Se 1 componente, pronto
        if (components.Count <= 1) return;

        // Conecta componentes próximos
        while (components.Count > 1)
        {
            int best = int.MaxValue;
            int ci = -1, cj = -1;
            (int x, int y) pi = default, pj = default;

            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    foreach (var a in components[i])
                    {
                        foreach (var b in components[j])
                        {
                            int d = Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
                            if (d < best)
                            {
                                best = d;
                                ci = i;
                                cj = j;
                                pi = a;
                                pj = b;
                            }
                        }
                    }
                }
            }

            if (best == int.MaxValue) break;

            // Tenta rotar; se falhar, desenha bridge
            PathRouter.Connect(grid, paths, pi, pj, (x, y) => 0f);
            if (!paths[pj.x, pj.y])
            {
                BridgeDrawer.Draw(grid, paths, pi, pj, rng);
            }

            // Refaz BFS
            visited = new bool[w, h];
            components.Clear();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!paths[x, y] || visited[x, y]) continue;
                    var comp = new List<(int x, int y)>();
                    var q = new Queue<(int, int)>();
                    q.Enqueue((x, y));
                    visited[x, y] = true;
                    while (q.Count > 0)
                    {
                        var (cx, cy) = q.Dequeue();
                        comp.Add((cx, cy));
                        foreach (var (nx, ny) in GridUtilities.Adj4(cx, cy, w, h))
                        {
                            if (paths[nx, ny] && !visited[nx, ny])
                            {
                                visited[nx, ny] = true;
                                q.Enqueue((nx, ny));
                            }
                        }
                    }
                    components.Add(comp);
                }
            }
        }
    }

    // Tiles de borda por ilha (terra adjacente a água) — reduz o espaço de busca
    private static List<(int x, int y)>[] CollectBorders(bool[,] grid, int[,] labels, int count, int w, int h)
    {
        var borders = new List<(int x, int y)>[count + 1];
        for (int i = 1; i <= count; i++) borders[i] = [];

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            if (labels[x, y] == 0) continue;
            foreach (var (nx, ny) in GridUtilities.Adj4(x, y, w, h))
                if (!grid[nx, ny]) { borders[labels[x, y]].Add((x, y)); break; }
        }

        return borders;
    }

    // Para cada par de ilhas, o par de tiles mais próximos (Manhattan)
    private static List<(int dist, int a, int b, (int x, int y) pa, (int x, int y) pb)> FindClosestPairs(
        List<(int x, int y)>[] borders, int count)
    {
        var edges = new List<(int dist, int a, int b, (int x, int y) pa, (int x, int y) pb)>();

        for (int i = 1; i <= count; i++)
        for (int j = i + 1; j <= count; j++)
        {
            int best = int.MaxValue;
            (int x, int y) pa = default, pb = default;

            foreach (var a in borders[i])
            foreach (var b in borders[j])
            {
                int d = Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
                if (d < best) { best = d; pa = a; pb = b; }
            }

            if (best != int.MaxValue) edges.Add((best, i, j, pa, pb));
        }

        edges.Sort((a, b) => a.dist.CompareTo(b.dist));
        return edges;
    }
}
