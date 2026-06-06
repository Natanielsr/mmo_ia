import Phaser from 'phaser';
import type { ChunkData, MapObjectData } from '../types';
import { GRID_SIZE } from '../config/constants';
import { TileType, type BiomeIndex } from '../config/tileTypeRegistry';
import { getTerrainAssetEntry } from '../config/terrainAssetMap';

export class ChunkManager {
    private scene: Phaser.Scene;
    private loadedChunks: Map<string, { group: Phaser.GameObjects.Group, rt?: Phaser.GameObjects.RenderTexture }> = new Map();
    private visitedChunks: Set<string> = new Set();
    private chunkSize: number = 16;
    private allObjects: Map<string, MapObjectData[]> = new Map();
    private pendingRequests: Set<string> = new Set();
    private onChunksNeeded: (coords: { cx: number; cy: number }[]) => void;

    constructor(scene: Phaser.Scene, onChunksNeeded: (coords: { cx: number; cy: number }[]) => void) {
        this.scene = scene;
        this.onChunksNeeded = onChunksNeeded;
    }

    public getVisitedChunks() { return this.visitedChunks; }
    public getAllObjects() { return this.allObjects; }
    public getChunkSize() { return this.chunkSize; }

    public handleChunkLoaded(data: ChunkData) {
        const chunkKey = `${data.cx},${data.cy}`;
        this.pendingRequests.delete(chunkKey);
        this.visitedChunks.add(chunkKey);
        this.allObjects.set(chunkKey, data.objects);

        if (this.loadedChunks.has(chunkKey)) {
            return;
        }

        const group = this.scene.add.group();
        this.renderChunkObjects(data.objects, group);

        const rt = data.tiles ? this.renderChunkTerrain(data.cx, data.cy, data.tiles, data.biomes) : undefined;

        this.loadedChunks.set(chunkKey, { group, rt });
    }

    private renderChunkTerrain(
        cx: number, cy: number,
        tiles: number[][],
        biomes?: number[][]
    ): Phaser.GameObjects.RenderTexture {
        const size = this.chunkSize;
        const rtW  = size * GRID_SIZE;
        const rtH  = size * GRID_SIZE;
        // World coords: x positive right, y positive up (frontend Y-up convention)
        const rtX  =  cx       * size * GRID_SIZE;
        const rtY  = -(cy + 1) * size * GRID_SIZE;

        const rt = this.scene.add.renderTexture(rtX, rtY, rtW, rtH);
        rt.setOrigin(0, 0);
        rt.setDepth(-100000);

        const tmp = this.scene.make.image({ x: 0, y: 0, key: 'water_full', add: false });
        tmp.setOrigin(0, 0);

        for (let row = 0; row < size; row++) {
            for (let col = 0; col < size; col++) {
                const tileType = tiles[row]?.[col] ?? TileType.WaterFull;
                const biome    = ((biomes?.[row]?.[col] ?? 0)) as BiomeIndex;

                let key: string;
                let flipY = true;
                if (tileType === TileType.WaterFull) {
                    key   = 'water_full';
                    flipY = false;
                } else {
                    const entry = getTerrainAssetEntry(tileType, biome);
                    key = entry ? entry.key : 'water_full';
                }

                tmp.setTexture(key);
                tmp.setFlipY(flipY);
                tmp.setDisplaySize(GRID_SIZE, GRID_SIZE);

                const localX = col * GRID_SIZE;
                const localY = (size - 1 - row) * GRID_SIZE; // inverts row order (Y-up)
                rt.draw(tmp, localX, localY);
            }
        }

        tmp.destroy();
        return rt;
    }

    private renderChunkObjects(objects: MapObjectData[], group: Phaser.GameObjects.Group) {
        objects.forEach(obj => {
            const worldX = obj.position.x * GRID_SIZE + (GRID_SIZE / 2);
            const worldY = -obj.position.y * GRID_SIZE - (GRID_SIZE / 2);

            const sprite = this.scene.add.sprite(worldX, worldY, obj.objectCode);
            sprite.setDisplaySize(GRID_SIZE, GRID_SIZE);
            sprite.setDepth(worldY);
            group.add(sprite);
        });
    }

    public update(playerPos: { x: number, y: number }, loadRadius: number = 2) {
        const pcx = Math.floor(playerPos.x / this.chunkSize);
        const pcy = Math.floor(playerPos.y / this.chunkSize);

        const keysToRemove: string[] = [];
        const safetyMargin = 1;

        this.loadedChunks.forEach((_, key) => {
            const [cx, cy] = key.split(',').map(Number);
            const dist = Math.max(Math.abs(cx - pcx), Math.abs(cy - pcy));

            if (dist > loadRadius + safetyMargin) {
                keysToRemove.push(key);
            }
        });

        keysToRemove.forEach(key => {
            const chunk = this.loadedChunks.get(key);
            if (chunk) {
                chunk.group.destroy(true);
                chunk.rt?.destroy();
                this.loadedChunks.delete(key);
            }
        });

        if (this.allObjects.size > 100) {
            const allKeys = Array.from(this.allObjects.keys());
            for (let i = 0; i < allKeys.length - 100; i++) {
                this.allObjects.delete(allKeys[i]);
                this.visitedChunks.delete(allKeys[i]);
            }
        }

        // Detecta chunks visíveis faltantes e solicita ao servidor
        const missing: { cx: number; cy: number }[] = [];
        for (let dx = -loadRadius; dx <= loadRadius; dx++) {
            for (let dy = -loadRadius; dy <= loadRadius; dy++) {
                const cx = pcx + dx;
                const cy = pcy + dy;
                const key = `${cx},${cy}`;
                if (!this.loadedChunks.has(key) && !this.pendingRequests.has(key)) {
                    missing.push({ cx, cy });
                    this.pendingRequests.add(key);
                }
            }
        }
        if (missing.length > 0) this.onChunksNeeded(missing);
    }

    public clear() {
        this.loadedChunks.forEach(chunk => {
            chunk.group.destroy(true);
            chunk.rt?.destroy();
        });
        this.loadedChunks.clear();
        this.pendingRequests.clear();
    }
}
