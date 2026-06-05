import { describe, it, expect } from 'vitest';
import { getTerrainAssetEntry, getAllTerrainAssetEntries } from '../../config/terrainAssetMap';
import { TileType } from '../../config/tileTypeRegistry';
import type { BiomeIndex } from '../../config/tileTypeRegistry';

// Enumera os PNGs reais da pasta public/assets/terrain em tempo de build (Vite glob).
const modules = import.meta.glob('../../../public/assets/terrain/**/*.png');
const available = new Set(
    Object.keys(modules).map((p) => p.slice(p.indexOf('/public/') + '/public/'.length)),
);

const biomes: BiomeIndex[] = [0, 1, 2];
const tileTypes = Object.values(TileType).filter((v) => typeof v === 'number') as number[];

describe('terrainAssetMap', () => {
    it('discovers terrain assets via glob', () => {
        expect(available.size).toBeGreaterThan(0);
        expect(available.has('assets/terrain/water_full.png')).toBe(true);
    });

    it('WaterFull resolves to null for every biome', () => {
        for (const b of biomes) {
            expect(getTerrainAssetEntry(TileType.WaterFull, b)).toBeNull();
        }
    });

    it('every (tileType, biome) resolves to an existing asset file (null only for WaterFull)', () => {
        for (const tt of tileTypes) {
            for (const b of biomes) {
                const entry = getTerrainAssetEntry(tt, b);
                if (entry === null) {
                    expect(tt).toBe(TileType.WaterFull);
                    continue;
                }
                expect(available.has(entry.path), `missing asset tile=${tt} biome=${b}: ${entry.path}`).toBe(true);
            }
        }
    });

    it('getAllTerrainAssetEntries returns unique keys mapped to existing files', () => {
        const entries = getAllTerrainAssetEntries();
        const keys = new Set<string>();
        for (const e of entries) {
            expect(keys.has(e.key)).toBe(false);
            keys.add(e.key);
            expect(available.has(e.path), `missing ${e.path}`).toBe(true);
        }
    });
});
