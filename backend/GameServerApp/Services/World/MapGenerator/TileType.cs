namespace GameServerApp.Services.World.MapGenerator;

public enum TileType
{
    WaterFull,
    TerrainFull,

    // 1 canto de terreno (ilha pequena)
    TerrainWaterCornerNW,  // só TL
    TerrainWaterCornerNE,  // só TR
    TerrainWaterCornerSW,  // só BL
    TerrainWaterCornerSE,  // só BR

    // 2 cantos adjacentes de terreno (borda reta)
    WaterTerrainEdgeN,     // TL + TR
    WaterTerrainEdgeS,     // BL + BR
    WaterTerrainEdgeE,     // TR + BR
    WaterTerrainEdgeW,     // TL + BL

    // 3 cantos de terreno (recuo de água em canto)
    WaterTerrainCornerNW,  // TR + BL + BR  (água falta NW/TL)
    WaterTerrainCornerNE,  // TL + BL + BR  (água falta NE/TR)
    WaterTerrainCornerSW,  // TL + TR + BR  (água falta SW/BL)
    WaterTerrainCornerSE,  // TL + TR + BL  (água falta SE/BR)

    // ── Caminho ────────────────────────────────────────────────────────────
    PathFull,

    // 1 canto de terreno (caminho domina)
    TerrainPathCornerNW,  // só BR
    TerrainPathCornerNE,  // só BL
    TerrainPathCornerSW,  // só TR
    TerrainPathCornerSE,  // só TL

    // 2 cantos adjacentes de terreno (borda reta de caminho)
    PathTerrainEdgeN,     // TL + TR
    PathTerrainEdgeS,     // BL + BR
    PathTerrainEdgeE,     // TR + BR
    PathTerrainEdgeW,     // TL + BL

    // 3 cantos de terreno (recuo de caminho em canto)
    PathTerrainCornerNW,  // TL + TR + BL  (caminho em BR)
    PathTerrainCornerNE,  // TL + TR + BR  (caminho em BL)
    PathTerrainCornerSW,  // TL + BL + BR  (caminho em TR)
    PathTerrainCornerSE,  // TR + BL + BR  (caminho em TL)

    // Diagonais estreitas de caminho
    PathTerrainCrossNWtoSE,  // caminho em TL + BR  (terreno em TR + BL)
    PathTerrainCrossNEtoSW,  // caminho em TR + BL  (terreno em TL + BR)
}
