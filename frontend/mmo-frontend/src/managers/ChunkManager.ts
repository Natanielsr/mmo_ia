import Phaser from 'phaser';
import type { ChunkData, MapObjectData } from '../types';
import type { WorldMapData } from '../dtos/world';
import { GRID_SIZE } from '../config/constants';
import { TileType } from '../config/tileTypeRegistry';
import type { BiomeIndex } from '../config/tileTypeRegistry';
import { getTerrainAssetEntry } from '../config/terrainAssetMap';

export class ChunkManager {
    private scene: Phaser.Scene;
    private loadedChunks: Map<string, { group: Phaser.GameObjects.Group, rt?: Phaser.GameObjects.RenderTexture }> = new Map();
    private visitedChunks: Set<string> = new Set();
    private chunkSize: number = 16;
    private allObjects: Map<string, MapObjectData[]> = new Map();
    private worldRts: Phaser.GameObjects.RenderTexture[] = [];

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public getVisitedChunks() { return this.visitedChunks; }
    public getAllObjects() { return this.allObjects; }
    public getChunkSize() { return this.chunkSize; }

    public handleChunkLoaded(data: ChunkData) {
        const chunkKey = `${data.cx},${data.cy}`;
        this.visitedChunks.add(chunkKey);
        this.allObjects.set(chunkKey, data.objects);

        // Se o chunk já está renderizado, não faz nada
        if (this.loadedChunks.has(chunkKey)) {
            return;
        }

        const group = this.scene.add.group();

        // Renderiza apenas Objetos Estáticos (Árvores, Pedras, etc.).
        // O terreno é renderizado de uma vez via renderWorldMap (matriz FractalRiver).
        this.renderChunkObjects(data.objects, group);

        this.loadedChunks.set(chunkKey, { group });
    }

    /**
     * Renderiza o mapa inteiro a partir da matriz do FractalRiver numa única RenderTexture.
     * O backend usa Y-down (DualGrid) e o frontend Y-up: além de inverter a posição das linhas,
     * o conteúdo de cada tile é espelhado verticalmente (flipY) para alinhar a artwork.
     */
    public renderWorldMap(map: WorldMapData) {
        const { cols, rows, tiles, biomes } = map;
        if (!cols || !rows) return;

        this.clearWorldRts();

        // Divide o mundo em blocos para nunca estourar o limite de textura da GPU.
        // BLOCK*GRID_SIZE = 2048px por RenderTexture (seguro em qualquer hardware).
        const BLOCK = 32;

        const tmp = this.scene.make.image({ x: 0, y: 0, key: 'water_full', add: false });
        tmp.setOrigin(0, 0);

        for (let by = 0; by < rows; by += BLOCK) {
            for (let bx = 0; bx < cols; bx += BLOCK) {
                const bw = Math.min(BLOCK, cols - bx);
                const bh = Math.min(BLOCK, rows - by);

                // Bloco cobre worldX [bx*GRID, (bx+bw)*GRID], worldY [-(by+bh)*GRID, -by*GRID].
                const rt = this.scene.add.renderTexture(
                    bx * GRID_SIZE, -(by + bh) * GRID_SIZE, bw * GRID_SIZE, bh * GRID_SIZE,
                );
                rt.setOrigin(0, 0);
                rt.setDepth(-100000);

                for (let ly = 0; ly < bh; ly++) {
                    for (let lx = 0; lx < bw; lx++) {
                        const x = bx + lx;
                        const y = by + ly;
                        const tileType = tiles[y]?.[x] ?? TileType.TerrainFull;
                        const biome = (biomes[y]?.[x] ?? 0) as BiomeIndex;

                        let key: string;
                        let flipY = true; // espelha N↔S da artwork (backend Y-down → frontend Y-up)
                        if (tileType === TileType.WaterFull) {
                            key = 'water_full';
                            flipY = false; // tile cheio, simétrico
                        } else {
                            const entry = getTerrainAssetEntry(tileType, biome);
                            key = entry ? entry.key : 'water_full';
                        }

                        tmp.setTexture(key);
                        tmp.setFlipY(flipY);
                        tmp.setDisplaySize(GRID_SIZE, GRID_SIZE);

                        const localX = lx * GRID_SIZE;
                        const localY = (bh - 1 - ly) * GRID_SIZE; // inverte ordem das linhas (Y-up)
                        rt.draw(tmp, localX, localY);
                    }
                }

                this.worldRts.push(rt);
            }
        }

        tmp.destroy();
    }

    private clearWorldRts() {
        this.worldRts.forEach(rt => rt.destroy());
        this.worldRts = [];
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

    /**
     * Limpa chunks que estão muito distantes do jogador para economizar memória e CPU
     */
    public update(playerPos: { x: number, y: number }, loadRadius: number = 2) {
        const pcx = Math.floor(playerPos.x / this.chunkSize);
        const pcy = Math.floor(playerPos.y / this.chunkSize);

        const keysToRemove: string[] = [];
        const safetyMargin = 1; // Mantém um pouco mais do que o necessário para evitar flicker

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

        // Otimização de Memória: Limita o cache global de objetos se ficar muito grande
        // Mantemos os últimos 100 chunks visitados para não pesar a memória
        if (this.allObjects.size > 100) {
            const allKeys = Array.from(this.allObjects.keys());
            // Remove os mais antigos (abordagem simples)
            for (let i = 0; i < allKeys.length - 100; i++) {
                this.allObjects.delete(allKeys[i]);
                this.visitedChunks.delete(allKeys[i]);
            }
        }
    }

    public clear() {
        this.loadedChunks.forEach(chunk => {
            chunk.group.destroy(true);
            chunk.rt?.destroy();
        });
        this.loadedChunks.clear();
        this.clearWorldRts();
    }
}
