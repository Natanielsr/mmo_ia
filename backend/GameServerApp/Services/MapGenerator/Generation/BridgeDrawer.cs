namespace FractalRiver.Generation;

internal static class BridgeDrawer
{
    internal static void Draw(bool[,] grid, bool[,] paths, (int x, int y) from, (int x, int y) to, Random rng, int halfWidth = 1)
    {
        int w  = grid.GetLength(0), h = grid.GetLength(1);
        int dx = to.x - from.x, dy = to.y - from.y;
        int len = Math.Abs(dx) + Math.Abs(dy);

        if (len <= 4)
        {
            DrawLPath(from.x, from.y, to.x, to.y);
            return;
        }

        // Ponto médio deslocado perpendicularmente: cria uma curva em arco
        float perpLen   = MathF.Sqrt(dx * dx + dy * dy);
        float amplitude = len * (0.15f + rng.NextSingle() * 0.2f);
        int   sign      = rng.Next(2) * 2 - 1;

        int midX = Math.Clamp((from.x + to.x) / 2 + (int)(-dy / perpLen * amplitude * sign), 0, w - 1);
        int midY = Math.Clamp((from.y + to.y) / 2 + (int)( dx / perpLen * amplitude * sign), 0, h - 1);

        DrawLPath(from.x, from.y, midX, midY);
        DrawLPath(midX, midY, to.x, to.y);

        void DrawLPath(int fx, int fy, int tx, int ty)
        {
            int x = fx, y = fy;
            for (int d = -halfWidth; d <= halfWidth; d++) { Mark(x, y + d); Mark(x + d, y); }
            while (x != tx) { x += Math.Sign(tx - x); for (int d = -halfWidth; d <= halfWidth; d++) Mark(x, y + d); }
            while (y != ty) { y += Math.Sign(ty - y); for (int d = -halfWidth; d <= halfWidth; d++) Mark(x + d, y); }
        }

        void Mark(int mx, int my)
        {
            if ((uint)mx < (uint)w && (uint)my < (uint)h)
            { grid[mx, my] = true; paths[mx, my] = true; }
        }
    }
}
