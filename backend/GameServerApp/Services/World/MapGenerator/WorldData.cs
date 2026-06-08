namespace GameServerApp.Services.World.MapGenerator;

public record WorldData(int Cols, int Rows, TileType[,] Tiles, Biome[,] Biomes)
{
    /// <summary>
    /// Jagged array para serialização JSON (ex: SignalR).
    /// tiles[row][col] = (int)TileType
    /// </summary>
    public int[][] TilesAsJaggedArray()
    {
        var result = new int[Rows][];
        for (int r = 0; r < Rows; r++)
        {
            result[r] = new int[Cols];
            for (int c = 0; c < Cols; c++)
                result[r][c] = (int)Tiles[c, r];
        }
        return result;
    }

    /// <summary>
    /// Jagged array para serialização JSON (ex: SignalR).
    /// biomes[row][col] = (int)Biome
    /// </summary>
    public int[][] BiomesAsJaggedArray()
    {
        var result = new int[Rows][];
        for (int r = 0; r < Rows; r++)
        {
            result[r] = new int[Cols];
            for (int c = 0; c < Cols; c++)
                result[r][c] = (int)Biomes[c, r];
        }
        return result;
    }
}
