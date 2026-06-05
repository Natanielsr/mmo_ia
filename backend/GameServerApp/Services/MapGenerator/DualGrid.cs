namespace FractalRiver;

public static class DualGrid
{
    // Encoding: bit0=TL, bit1=TR, bit2=BL, bit3=BR
    // Os tiles de canto usam convenção OPOSTA (NE↔SW, NW↔SE) — nomes trocados para compensar.
    // Y cresce para baixo (tela); TL → norte, BL → sul.

    // bit=1 = terreno, bit=0 = água
    private static readonly TileType[] Lookup =
    [
        TileType.WaterFull,              // 0000
        TileType.TerrainWaterCornerSE,   // 0001  só TL
        TileType.TerrainWaterCornerSW,   // 0010  só TR
        TileType.WaterTerrainEdgeN,      // 0011  TL + TR
        TileType.TerrainWaterCornerNE,   // 0100  só BL
        TileType.WaterTerrainEdgeW,      // 0101  TL + BL
        TileType.WaterFull,              // 0110  TR + BL  (diagonal — fallback água)
        TileType.WaterTerrainCornerNW,   // 0111  TL + TR + BL
        TileType.TerrainWaterCornerNW,   // 1000  só BR
        TileType.WaterFull,              // 1001  TL + BR  (diagonal — fallback água)
        TileType.WaterTerrainEdgeE,      // 1010  TR + BR
        TileType.WaterTerrainCornerNE,   // 1011  TL + TR + BR
        TileType.WaterTerrainEdgeS,      // 1100  BL + BR
        TileType.WaterTerrainCornerSW,   // 1101  TL + BL + BR
        TileType.WaterTerrainCornerSE,   // 1110  TR + BL + BR
        TileType.TerrainFull,            // 1111
    ];

    // bit=1 = terreno (não-caminho), bit=0 = caminho
    private static readonly TileType[] PathLookup =
    [
        TileType.PathFull,               // 0000
        TileType.TerrainPathCornerSE,    // 0001  só TL
        TileType.TerrainPathCornerSW,    // 0010  só TR
        TileType.PathTerrainEdgeN,       // 0011  TL + TR
        TileType.TerrainPathCornerNE,    // 0100  só BL
        TileType.PathTerrainEdgeW,       // 0101  TL + BL
        TileType.PathTerrainCrossNWtoSE, // 0110  TL + BR  (diagonal NW→SE)
        TileType.PathTerrainCornerNW,    // 0111  TL + TR + BL
        TileType.TerrainPathCornerNW,    // 1000  só BR
        TileType.PathTerrainCrossNEtoSW, // 1001  TR + BL  (diagonal NE→SW)
        TileType.PathTerrainEdgeE,       // 1010  TR + BR
        TileType.PathTerrainCornerNE,    // 1011  TL + TR + BR
        TileType.PathTerrainEdgeS,       // 1100  BL + BR
        TileType.PathTerrainCornerSW,    // 1101  TL + BL + BR
        TileType.PathTerrainCornerSE,    // 1110  TR + BL + BR
        TileType.TerrainFull,            // 1111
    ];

    /// <summary>
    /// Converte uma grade booleana de terreno em tiles dual-grid.
    /// isGrass deve ter tamanho (W+1) × (H+1); o resultado tem tamanho W × H.
    /// Cada célula display (x,y) observa os 4 cantos da grade world:
    /// TL=(x,y)  TR=(x+1,y)  BL=(x,y+1)  BR=(x+1,y+1).
    /// </summary>
    public static TileType[,] Compute(bool[,] isGrass)
    {
        int w = isGrass.GetLength(0) - 1;
        int h = isGrass.GetLength(1) - 1;
        var tiles = new TileType[w, h];

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            int config = (isGrass[x,   y  ] ? 1 : 0)
                       | (isGrass[x+1, y  ] ? 2 : 0)
                       | (isGrass[x,   y+1] ? 4 : 0)
                       | (isGrass[x+1, y+1] ? 8 : 0);
            tiles[x, y] = Lookup[config];
        }

        return tiles;
    }

    /// <summary>
    /// Versão com 3 terrenos: água, terreno e caminho.
    /// isLand e isPath têm tamanho (W+1) × (H+1); o resultado tem W × H.
    /// Células com qualquer canto de água usam o lookup água-terreno (caminho = terreno).
    /// Células totalmente em terra usam o lookup caminho-terreno.
    /// </summary>
    public static TileType[,] Compute(bool[,] isLand, bool[,] isPath)
    {
        int w = isLand.GetLength(0) - 1;
        int h = isLand.GetLength(1) - 1;
        var tiles = new TileType[w, h];

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            bool tl = isLand[x,   y  ];
            bool tr = isLand[x+1, y  ];
            bool bl = isLand[x,   y+1];
            bool br = isLand[x+1, y+1];

            if (!tl || !tr || !bl || !br)
            {
                int config = (tl ? 1 : 0) | (tr ? 2 : 0) | (bl ? 4 : 0) | (br ? 8 : 0);
                tiles[x, y] = Lookup[config];
            }
            else
            {
                // bit=1 = terreno (não-caminho), bit=0 = caminho
                int config = (!isPath[x,   y  ] ? 1 : 0)
                           | (!isPath[x+1, y  ] ? 2 : 0)
                           | (!isPath[x,   y+1] ? 4 : 0)
                           | (!isPath[x+1, y+1] ? 8 : 0);
                tiles[x, y] = PathLookup[config];
            }
        }

        return tiles;
    }
}
