import { TileType, getTerrainTextureKey } from './tileTypeRegistry';
import type { BiomeIndex } from './tileTypeRegistry';

interface AssetEntry {
  key: string;
  path: string;
}

// Returns asset key and path for given tile type and biome
export function getTerrainAssetEntry(tileType: number, biome: BiomeIndex): AssetEntry | null {
  // Special case: WaterFull has no asset, skip drawing
  if (tileType === TileType.WaterFull) {
    return null;
  }

  // PathFull is at terrain root, shared across all biomes
  if (tileType === TileType.PathFull) {
    return {
      key: getTerrainTextureKey(tileType, biome),
      path: 'assets/terrain/path_full.png',
    };
  }

  const key = getTerrainTextureKey(tileType, biome);
  let basePath = '';

  switch (biome) {
    case 0: // Grass
      basePath = 'assets/terrain/grass_biome';
      break;
    case 1: // Sand
      basePath = 'assets/terrain/sand_biome';
      break;
    case 2: // Snow
      basePath = 'assets/terrain/snow_biome';
      break;
  }

  const fileName = getTileFileName(tileType, biome);
  if (!fileName) return null;

  return {
    key,
    path: `${basePath}/${fileName}`,
  };
}

// Map tile type to filename based on biome
function getTileFileName(tileType: number, biome: BiomeIndex): string | null {
  const biomePrefix = biome === 0 ? 'grass' : biome === 1 ? 'sand' : 'snow';

  switch (tileType) {
    // Terrain full tiles
    case TileType.TerrainFull:
      return `${biomePrefix}_full.png`;

    // Water-terrain corners (terrain small corner)
    case TileType.TerrainWaterCornerNW:
      return `${biomePrefix}_water_corner_NW.png`;
    case TileType.TerrainWaterCornerNE:
      return `${biomePrefix}_water_corner_NE.png`;
    case TileType.TerrainWaterCornerSW:
      return `${biomePrefix}_water_corner_SW.png`;
    case TileType.TerrainWaterCornerSE:
      return `${biomePrefix}_water_corner_SE.png`;

    // Water-terrain edges
    case TileType.WaterTerrainEdgeN:
      return `water_${biomePrefix}_edge_N.png`;
    case TileType.WaterTerrainEdgeS:
      return `water_${biomePrefix}_edge_S.png`;
    case TileType.WaterTerrainEdgeE:
      return `water_${biomePrefix}_edge_E.png`;
    case TileType.WaterTerrainEdgeW:
      return `water_${biomePrefix}_edge_W.png`;

    // Water-terrain corners (water recessed)
    case TileType.WaterTerrainCornerNW:
      return `water_${biomePrefix}_corner_NW.png`;
    case TileType.WaterTerrainCornerNE:
      return `water_${biomePrefix}_corner_NE.png`;
    case TileType.WaterTerrainCornerSW:
      return `water_${biomePrefix}_corner_SW.png`;
    case TileType.WaterTerrainCornerSE:
      if (biomePrefix === 'grass') return 'water_grass__corner_SE.png'; // double underscore intentional
      return `water_${biomePrefix}_corner_SE.png`;

    // Water-sand diagonals (sand only)
    // Skipping for grass; using edges as fallback
    // case 'WaterSandCrossNWtoSE': handled as path variant

    // Path full tiles
    case TileType.PathFull:
      return 'path_full.png';

    // Terrain-path corners (terrain small corner)
    case TileType.TerrainPathCornerNW:
      return `${biomePrefix}_path_corner_NW.png`;
    case TileType.TerrainPathCornerNE:
      return `${biomePrefix}_path_corner_NE.png`;
    case TileType.TerrainPathCornerSW:
      return `${biomePrefix}_path_corner_SW.png`;
    case TileType.TerrainPathCornerSE:
      return `${biomePrefix}_path_corner_SE.png`;

    // Path-terrain edges
    case TileType.PathTerrainEdgeN:
      return `path_${biomePrefix}_edge_N.png`;
    case TileType.PathTerrainEdgeS:
      return `path_${biomePrefix}_edge_S.png`;
    case TileType.PathTerrainEdgeE:
      return `path_${biomePrefix}_edge_E.png`;
    case TileType.PathTerrainEdgeW:
      return `path_${biomePrefix}_edge_W.png`;

    // Path-terrain corners (path recessed)
    case TileType.PathTerrainCornerNW:
      return `path_${biomePrefix}_corner_NW.png`;
    case TileType.PathTerrainCornerNE:
      return `path_${biomePrefix}_corner_NE.png`;
    case TileType.PathTerrainCornerSW:
      return `path_${biomePrefix}_corner_SW.png`;
    case TileType.PathTerrainCornerSE:
      return `path_${biomePrefix}_corner_SE.png`;

    // Path crossings
    case TileType.PathTerrainCrossNWtoSE:
      return `path_${biomePrefix}_cross_NW_to_SE.png`;
    case TileType.PathTerrainCrossNEtoSW:
      return `path_${biomePrefix}_cross_NE_to_SW.png`;

    default:
      return null;
  }
}

// Get all terrain assets for preloading
export function getAllTerrainAssetEntries(): AssetEntry[] {
  const entries: AssetEntry[] = [];
  const biomes: BiomeIndex[] = [0, 1, 2];

  for (const tileType of Object.values(TileType).filter((v) => typeof v === 'number')) {
    for (const biome of biomes) {
      const entry = getTerrainAssetEntry(tileType as number, biome);
      if (entry && !entries.some((e) => e.key === entry.key)) {
        entries.push(entry);
      }
    }
  }

  return entries;
}
