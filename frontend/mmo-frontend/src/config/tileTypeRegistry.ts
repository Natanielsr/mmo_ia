// Must match FractalRiver.TileType enum ordinal values exactly
export const TileType = {
  WaterFull: 0,
  TerrainFull: 1,
  TerrainWaterCornerNW: 2,
  TerrainWaterCornerNE: 3,
  TerrainWaterCornerSW: 4,
  TerrainWaterCornerSE: 5,
  WaterTerrainEdgeN: 6,
  WaterTerrainEdgeS: 7,
  WaterTerrainEdgeE: 8,
  WaterTerrainEdgeW: 9,
  WaterTerrainCornerNW: 10,
  WaterTerrainCornerNE: 11,
  WaterTerrainCornerSW: 12,
  WaterTerrainCornerSE: 13,
  PathFull: 14,
  TerrainPathCornerNW: 15,
  TerrainPathCornerNE: 16,
  TerrainPathCornerSW: 17,
  TerrainPathCornerSE: 18,
  PathTerrainEdgeN: 19,
  PathTerrainEdgeS: 20,
  PathTerrainEdgeE: 21,
  PathTerrainEdgeW: 22,
  PathTerrainCornerNW: 23,
  PathTerrainCornerNE: 24,
  PathTerrainCornerSW: 25,
  PathTerrainCornerSE: 26,
  PathTerrainCrossNWtoSE: 27,
  PathTerrainCrossNEtoSW: 28,
} as const;

export type BiomeIndex = 0 | 1 | 2; // 0=Grass, 1=Sand, 2=Snow

export const getTerrainTextureKey = (t: number, b: BiomeIndex) => `terrain_${b}_${t}`;
