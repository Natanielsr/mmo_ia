namespace GameServerApp.Dtos
{
    /// <summary>
    /// Matriz completa do mundo gerado pelo FractalRiver, enviada ao frontend
    /// para renderizar o terreno inteiro de uma vez.
    /// Tiles/Biomes seguem ordem [row][col] (row = Y, col = X).
    /// </summary>
    public class WorldMapData
    {
        public int Cols { get; set; }
        public int Rows { get; set; }
        public int[][] Tiles { get; set; } = System.Array.Empty<int[]>();
        public int[][] Biomes { get; set; } = System.Array.Empty<int[]>();
    }
}
