import Phaser from 'phaser';
import type { ChunkData, MapObjectData } from '../types';
import { GRID_SIZE } from '../config/constants';

export class ChunkManager {
    private scene: Phaser.Scene;
    private loadedChunks: Map<string, { group: Phaser.GameObjects.Group, rt?: Phaser.GameObjects.RenderTexture }> = new Map();
    private visitedChunks: Set<string> = new Set();
    private chunkSize: number = 16;
    private allObjects: Map<string, MapObjectData[]> = new Map();

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
        
        // Otimização: RenderTexture para os tiles de grama
        // Isso "achata" 256 sprites em uma única imagem por chunk
        const rt = this.renderChunkTilesToTexture(data.cx, data.cy);
        
        this.loadedChunks.set(chunkKey, { group, rt });

        // Renderiza Objetos Estáticos (Árvores, Pedras, etc.)
        this.renderChunkObjects(data.objects, group);

        console.log(`[ChunkManager] Chunk otimizado carregado: ${chunkKey} (${data.objects.length} objetos).`);
    }

    private renderChunkTilesToTexture(cx: number, cy: number): Phaser.GameObjects.RenderTexture {
        const sizePx = this.chunkSize * GRID_SIZE;
        const worldX = cx * sizePx;
        const worldY = -cy * sizePx;

        const rt = this.scene.add.renderTexture(worldX + sizePx / 2, worldY - sizePx / 2, sizePx, sizePx);
        rt.setOrigin(0.5, 0.5);
        rt.setDepth(-100000);

        // Sprite temporário reutilizável para desenhar cada tile no tamanho correto
        const tmpSprite = this.scene.make.image({ x: 0, y: 0, key: 'grass1', add: false });
        tmpSprite.setDisplaySize(GRID_SIZE, GRID_SIZE);
        tmpSprite.setOrigin(0, 0);

        for (let x = 0; x < this.chunkSize; x++) {
            for (let y = 0; y < this.chunkSize; y++) {
                const absX = Math.abs(cx * this.chunkSize + x);
                const absY = Math.abs(cy * this.chunkSize + y);
                const seed = (absX * 1000 + absY) % 12;
                const grassType = `grass${seed + 1}`;

                tmpSprite.setTexture(grassType);
                tmpSprite.setDisplaySize(GRID_SIZE, GRID_SIZE);

                const localX = x * GRID_SIZE;
                const localY = (this.chunkSize - 1 - y) * GRID_SIZE;

                rt.draw(tmpSprite, localX, localY);
            }
        }

        tmpSprite.destroy();
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
                console.log(`[ChunkManager] Chunk ${key} removido por distância.`);
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
    }
}
