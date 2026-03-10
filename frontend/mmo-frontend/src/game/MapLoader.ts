import Phaser from 'phaser';
import type { MapObjectData } from '../types';

const GRID_SIZE = 64;

export class MapLoader {
    private static instance: MapLoader;
    private mapObjects: MapObjectData[] = [];

    private constructor() { }

    public static getInstance(): MapLoader {
        if (!MapLoader.instance) {
            MapLoader.instance = new MapLoader();
        }
        return MapLoader.instance;
    }

    public async loadMapObjects(): Promise<MapObjectData[]> {
        try {
            const response = await fetch('http://localhost:5258/api/map/objects');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const mapObjects: MapObjectData[] = await response.json();
            this.mapObjects = mapObjects;

            console.log("Carregados objetos do mapa:", mapObjects);
            return mapObjects;
        } catch (error) {
            console.error('Erro ao carregar objetos do mapa:', error);
            return [];
        }
    }

    public getMapObjects(): MapObjectData[] {
        return this.mapObjects;
    }

    public renderMapObjects(scene: Phaser.Scene): void {
        // Cria sprites para cada objeto do mapa
        this.mapObjects.forEach(object => {
            // Calcula coordenadas mundo baseadas na posição do objeto
            const worldX = object.position.x * GRID_SIZE + (GRID_SIZE / 2);
            const worldY = -object.position.y * GRID_SIZE - (GRID_SIZE / 2);

            // Aqui você pode criar diferentes sprites dependendo do tipo de objeto
            // Por agora vamos usar um quadrado colorido simples
            const color = this.getObjectColor(object.objectCode);
            const graphics = scene.add.graphics();
            graphics.fillStyle(color, 1);
            graphics.fillRect(worldX - GRID_SIZE / 2, worldY - GRID_SIZE / 2, GRID_SIZE, GRID_SIZE);

            // Para melhor visibilidade do objeto, vamos adicionar uma borda
            graphics.lineStyle(2, 0x000000, 1);
            graphics.strokeRect(worldX - GRID_SIZE / 2, worldY - GRID_SIZE / 2, GRID_SIZE, GRID_SIZE);

            // Define uma profundidade baixa para os objetos do mapa (mais atrás)
            graphics.setDepth(100); // Profundidade menor

            // Armazena referência para controle futuro (opcional)
            (graphics as any).objectId = object.id;
        });
    }

    private getObjectColor(objectCode: string): number {
        // "Tree", "Rock", "Bush", "Pillar"
        switch (objectCode) {
            case 'Tree':
                return 0x8B4513; // Marrom para arvores
            case 'Rock':
                return 0x757575; // Cinza para obstáculos
            case 'Bush':
                return 0x008000; // Verde para arbusto
            case 'Pillar':
                return 0xFFFFFF; // Branco para pilares
            default:

                return 0x008000; // Verde para outros obstáculos
        }
    }
}