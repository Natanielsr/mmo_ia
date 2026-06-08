namespace GameServerApp.Services.World.MapGenerator.Generation;

internal static class GridUtilities
{
    internal static IEnumerable<(int x, int y)> Adj4(int x, int y, int w, int h)
    {
        if (x > 0)     yield return (x - 1, y);
        if (x < w - 1) yield return (x + 1, y);
        if (y > 0)     yield return (x, y - 1);
        if (y < h - 1) yield return (x, y + 1);
    }
}
