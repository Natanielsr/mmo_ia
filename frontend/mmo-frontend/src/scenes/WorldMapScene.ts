import Phaser from 'phaser';
import { MainScene } from './MainScene';

export class WorldMapScene extends Phaser.Scene {
    private mainScene!: MainScene;
    private graphics!: Phaser.GameObjects.Graphics;
    private tileSize: number = 6; // pixels per grid cell on map
    private isVisible: boolean = false;
    private backgroundRect!: Phaser.GameObjects.Rectangle;

    constructor() {
        super({ key: 'WorldMapScene' });
    }

    create() {
        this.mainScene = this.scene.get('MainScene') as MainScene;
        
        // Fundo escurecido para todo o mapa
        this.backgroundRect = this.add.rectangle(
            this.cameras.main.centerX,
            this.cameras.main.centerY,
            this.cameras.main.width * 0.8,
            this.cameras.main.height * 0.8,
            0x000000,
            0.85
        );
        this.backgroundRect.setStrokeStyle(2, 0x475569);
        
        this.graphics = this.add.graphics();
        
        this.add.text(
            this.cameras.main.centerX,
            this.cameras.main.centerY - (this.backgroundRect.height / 2) + 20,
            'WORLD MAP',
            { fontSize: '24px', color: '#f8fafc', fontStyle: 'bold' }
        ).setOrigin(0.5);

        this.add.text(
            this.cameras.main.centerX,
            this.cameras.main.centerY + (this.backgroundRect.height / 2) - 20,
            'M to close | Green: Players | Red: Enemies | Yellow: Items | Grey: Objects',
            { fontSize: '14px', color: '#94a3b8' }
        ).setOrigin(0.5);

        this.scene.setVisible(false);
    }

    public toggle() {
        this.isVisible = !this.isVisible;
        this.scene.setVisible(this.isVisible);
        if (this.isVisible) {
            this.scene.bringToTop();
            this.renderMap();
        }
    }

    update() {
        if (this.isVisible) {
            this.renderMap();
        }
    }

    private renderMap() {
        if (!this.graphics) return;

        this.graphics.clear();
        
        const myPlayer = this.mainScene.getMyPlayer();
        if (!myPlayer) return;

        // Centraliza o mapa no jogador



        const playerPos = myPlayer.gridPosition;
        // Desenha Chunks Visitados (Fundo)
        const visited = this.mainScene.chunkManager.getVisitedChunks();
        const chunkSize = this.mainScene.chunkManager.getChunkSize();
        const viewHalfW = (this.backgroundRect.width / this.tileSize) / 2;
        const viewHalfH = (this.backgroundRect.height / this.tileSize) / 2;

        // Otimização: Só processa chunks que estão dentro da visão do mapa
        const minCX = Math.floor((playerPos.x - viewHalfW) / chunkSize);
        const maxCX = Math.ceil((playerPos.x + viewHalfW) / chunkSize);
        const minCY = Math.floor((playerPos.y - viewHalfH) / chunkSize);
        const maxCY = Math.ceil((playerPos.y + viewHalfH) / chunkSize);

        // Desenha Fundo dos Chunks Visitados
        for (let cx = minCX; cx <= maxCX; cx++) {
            for (let cy = minCY; cy <= maxCY; cy++) {
                const key = `${cx},${cy}`;
                if (visited.has(key)) {
                    const startX = cx * chunkSize;
                    const startY = cy * chunkSize;
                    this.drawRect(startX, startY, chunkSize, chunkSize, playerPos, 0x0f172a);
                }
            }
        }

        // Desenha Objetos Estáticos
        const allObjects = this.mainScene.chunkManager.getAllObjects();
        for (let cx = minCX; cx <= maxCX; cx++) {
            for (let cy = minCY; cy <= maxCY; cy++) {
                const key = `${cx},${cy}`;
                const objects = allObjects.get(key);
                if (objects) {
                    objects.forEach(obj => {
                        this.drawCell(obj.position.x, obj.position.y, playerPos, 0x475569);
                    });
                }
            }
        }

        // Desenha Itens
        const items = this.mainScene.itemManager.getItemsDict();
        items.forEach((item: any) => {
            this.drawCell(item.gridPosition.x, item.gridPosition.y, playerPos, 0xeab308);
        });

        // Desenha Monstros
        const monsters = this.mainScene.monsterManager.getMonstersDict();
        Object.values(monsters).forEach(monster => {
            if (monster.isDead) return;
            this.drawCell(monster.gridPosition.x, monster.gridPosition.y, playerPos, 0xef4444);
        });

        // Desenha Outros Jogadores
        const players = (this.mainScene as any).playerManager.getPlayersDict();
        Object.values(players).forEach((player: any) => {
            if (player.id === myPlayer.id) return;
            this.drawCell(player.gridPosition.x, player.gridPosition.y, playerPos, 0x22c55e);
        });

        // Desenha Meu Player (Sempre por cima)
        this.drawCell(playerPos.x, playerPos.y, playerPos, 0x22c55e);
    }

    private drawCell(gx: number, gy: number, centerPos: {x: number, y: number}, color: number) {
        // Coordenadas relativas ao centro (jogador)
        const relX = gx - centerPos.x;
        const relY = -(gy - centerPos.y); // Inverte Y pois grid coord cresce para cima

        const viewHalfW = (this.backgroundRect.width / this.tileSize) / 2;
        const viewHalfH = (this.backgroundRect.height / this.tileSize) / 2;

        if (Math.abs(relX) > viewHalfW || Math.abs(relY) > viewHalfH) return;

        const px = this.cameras.main.centerX + relX * this.tileSize;
        const py = this.cameras.main.centerY + relY * this.tileSize;

        this.graphics.fillStyle(color, 1);
        this.graphics.fillRect(px - this.tileSize / 2, py - this.tileSize / 2, this.tileSize, this.tileSize);
    }

    private drawRect(gx: number, gy: number, gw: number, gh: number, centerPos: {x: number, y: number}, color: number) {
        // Coordenadas relativas ao centro
        const relXStart = gx - centerPos.x;
        const relYStart = -(gy - centerPos.y); // Inverte Y pois grid coord cresce para cima
        
        // No grid, se Y cresce para cima, o topo do chunk (gy + gh) é o menor Y na tela
        const relXEnd = relXStart + gw;
        const relYEnd = relYStart - gh;

        const pxStart = this.cameras.main.centerX + relXStart * this.tileSize;
        const pyStart = this.cameras.main.centerY + relYStart * this.tileSize;
        const pxEnd = this.cameras.main.centerX + relXEnd * this.tileSize;
        const pyEnd = this.cameras.main.centerY + relYEnd * this.tileSize;

        // Limita ao backgroundRect (aproximado)
        const left = this.cameras.main.centerX - this.backgroundRect.width / 2;
        const right = this.cameras.main.centerX + this.backgroundRect.width / 2;
        const top = this.cameras.main.centerY - this.backgroundRect.height / 2;
        const bottom = this.cameras.main.centerY + this.backgroundRect.height / 2;

        const x = Math.max(left, Math.min(pxStart, pxEnd));
        const y = Math.max(top, Math.min(pyStart, pyEnd));
        const w = Math.min(right, Math.max(pxStart, pxEnd)) - x;
        const h = Math.min(bottom, Math.max(pyStart, pyEnd)) - y;

        if (w > 0 && h > 0) {
            this.graphics.fillStyle(color, 1);
            this.graphics.fillRect(x, y, w, h);
        }
    }
}
