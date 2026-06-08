using GameServerApp.Services.World.MapGenerator;
using Xunit;

namespace FractalRiver.Tests;

// Valida que cada configuração de 4 cantos (TL/TR/BL/BR) produz o TileType correto.
// Encoding: bit0=TL, bit1=TR, bit2=BL, bit3=BR  (1=terreno, 0=água)
// config = TL*1 | TR*2 | BL*4 | BR*8
public class DualGridTests
{
    // isGrass 2×2 produz grade de tile 1×1: tiles[0,0]
    // isGrass[x,y]: TL=(0,0)  TR=(1,0)  BL=(0,1)  BR=(1,1)
    private static TileType ComputeSingle(bool tl, bool tr, bool bl, bool br)
    {
        var isGrass = new bool[2, 2];
        isGrass[0, 0] = tl;
        isGrass[1, 0] = tr;
        isGrass[0, 1] = bl;
        isGrass[1, 1] = br;
        return DualGrid.Compute(isGrass)[0, 0];
    }

    [Theory]
    // config 0 (0000) – tudo água
    [InlineData(false, false, false, false, TileType.WaterFull)]
    // config 1 (0001) – só TL
    [InlineData(true,  false, false, false, TileType.TerrainWaterCornerSE)]
    // config 2 (0010) – só TR
    [InlineData(false, true,  false, false, TileType.TerrainWaterCornerSW)]
    // config 3 (0011) – TL + TR (borda norte)
    [InlineData(true,  true,  false, false, TileType.WaterTerrainEdgeN)]
    // config 4 (0100) – só BL
    [InlineData(false, false, true,  false, TileType.TerrainWaterCornerNE)]
    // config 5 (0101) – TL + BL (borda oeste)
    [InlineData(true,  false, true,  false, TileType.WaterTerrainEdgeW)]
    // config 6 (0110) – TR + BL (diagonal → fallback água)
    [InlineData(false, true,  true,  false, TileType.WaterFull)]
    // config 7 (0111) – TL + TR + BL (recuo NW)
    [InlineData(true,  true,  true,  false, TileType.WaterTerrainCornerNW)]
    // config 8 (1000) – só BR
    [InlineData(false, false, false, true,  TileType.TerrainWaterCornerNW)]
    // config 9 (1001) – TL + BR (diagonal → fallback água)
    [InlineData(true,  false, false, true,  TileType.WaterFull)]
    // config 10 (1010) – TR + BR (borda leste)
    [InlineData(false, true,  false, true,  TileType.WaterTerrainEdgeE)]
    // config 11 (1011) – TL + TR + BR (recuo NE)
    [InlineData(true,  true,  false, true,  TileType.WaterTerrainCornerNE)]
    // config 12 (1100) – BL + BR (borda sul)
    [InlineData(false, false, true,  true,  TileType.WaterTerrainEdgeS)]
    // config 13 (1101) – TL + BL + BR (recuo SW)
    [InlineData(true,  false, true,  true,  TileType.WaterTerrainCornerSW)]
    // config 14 (1110) – TR + BL + BR (recuo SE)
    [InlineData(false, true,  true,  true,  TileType.WaterTerrainCornerSE)]
    // config 15 (1111) – tudo terreno
    [InlineData(true,  true,  true,  true,  TileType.TerrainFull)]
    public void Compute_CornerConfig_ReturnsTileCorreto(
        bool tl, bool tr, bool bl, bool br, TileType expected)
    {
        Assert.Equal(expected, ComputeSingle(tl, tr, bl, br));
    }

    [Fact]
    public void Compute_ResultadoTemDimensoes_ColunasMenosUmPorLinhasMenosUm()
    {
        int w = 5, h = 4;
        var isGrass = new bool[w + 1, h + 1];
        var tiles = DualGrid.Compute(isGrass);

        Assert.Equal(w, tiles.GetLength(0));
        Assert.Equal(h, tiles.GetLength(1));
    }
}

// Valida o overload Compute(isLand, isPath): todos os 16 configs do lookup caminho-terreno,
// diagonais, e o fallback para água quando algum canto não é terra.
// Encoding: bit0=TL, bit1=TR, bit2=BL, bit3=BR  (1=terreno, 0=caminho) — mesma lógica do lookup água-terreno.
// No helper: pathXx=true → esse canto é caminho; false → terreno.
public class DualGridPathTests
{
    private static TileType ComputePathSingle(bool pathTl, bool pathTr, bool pathBl, bool pathBr)
    {
        var isLand = new bool[2, 2];
        isLand[0, 0] = true; isLand[1, 0] = true;
        isLand[0, 1] = true; isLand[1, 1] = true;

        var isPath = new bool[2, 2];
        isPath[0, 0] = pathTl; isPath[1, 0] = pathTr;
        isPath[0, 1] = pathBl; isPath[1, 1] = pathBr;

        return DualGrid.Compute(isLand, isPath)[0, 0];
    }

    [Theory]
    // 0 cantos de caminho → tudo terreno
    [InlineData(false, false, false, false, TileType.TerrainFull)]
    // 4 cantos de caminho → tudo caminho
    [InlineData(true,  true,  true,  true,  TileType.PathFull)]
    // 1 canto de caminho (caminho aparece num único canto)
    [InlineData(true,  false, false, false, TileType.PathTerrainCornerSE)]  // só TL
    [InlineData(false, true,  false, false, TileType.PathTerrainCornerSW)]  // só TR
    [InlineData(false, false, true,  false, TileType.PathTerrainCornerNE)]  // só BL
    [InlineData(false, false, false, true,  TileType.PathTerrainCornerNW)]  // só BR
    // 3 cantos de caminho (terreno aparece num único canto)
    [InlineData(false, true,  true,  true,  TileType.TerrainPathCornerSE)]  // terreno só TL
    [InlineData(true,  false, true,  true,  TileType.TerrainPathCornerSW)]  // terreno só TR
    [InlineData(true,  true,  false, true,  TileType.TerrainPathCornerNE)]  // terreno só BL
    [InlineData(true,  true,  true,  false, TileType.TerrainPathCornerNW)]  // terreno só BR
    // 2 cantos adjacentes de caminho (bordas retas)
    [InlineData(false, false, true,  true,  TileType.PathTerrainEdgeN)]     // terreno TL+TR (norte)
    [InlineData(true,  true,  false, false, TileType.PathTerrainEdgeS)]     // terreno BL+BR (sul)
    [InlineData(true,  false, true,  false, TileType.PathTerrainEdgeE)]     // terreno TR+BR (leste)
    [InlineData(false, true,  false, true,  TileType.PathTerrainEdgeW)]     // terreno TL+BL (oeste)
    // diagonais: caminho em cantos opostos
    [InlineData(true,  false, false, true,  TileType.PathTerrainCrossNWtoSE)] // caminho TL+BR (↘)
    [InlineData(false, true,  true,  false, TileType.PathTerrainCrossNEtoSW)] // caminho TR+BL (↙)
    public void Compute_PathTerrain_Config_ReturnsTileCorreto(
        bool pathTl, bool pathTr, bool pathBl, bool pathBr, TileType expected)
    {
        Assert.Equal(expected, ComputePathSingle(pathTl, pathTr, pathBl, pathBr));
    }

    [Fact]
    public void Compute_ComAguaPresente_UsaLookupAguaIgnorandoCaminho()
    {
        // TL=água, TR+BL+BR=terra; TR é caminho mas deve ser tratado como terreno
        // config água-terreno: TL=0 TR=1 BL=1 BR=1 → 0b1110=14 → WaterTerrainCornerSE
        var isLand = new bool[2, 2];
        isLand[0, 0] = false; isLand[1, 0] = true;
        isLand[0, 1] = true;  isLand[1, 1] = true;

        var isPath = new bool[2, 2];
        isPath[1, 0] = true; // caminho em TR — deve ser ignorado pelo lookup

        Assert.Equal(TileType.WaterTerrainCornerSE, DualGrid.Compute(isLand, isPath)[0, 0]);
    }

    [Fact]
    public void Compute_PathLand_ResultadoTemDimensoesCorretas()
    {
        int w = 5, h = 4;
        var isLand = new bool[w + 1, h + 1];
        var isPath = new bool[w + 1, h + 1];
        var tiles  = DualGrid.Compute(isLand, isPath);

        Assert.Equal(w, tiles.GetLength(0));
        Assert.Equal(h, tiles.GetLength(1));
    }
}
