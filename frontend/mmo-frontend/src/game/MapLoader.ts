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

            // Usa object.objectCode como a chave (key) da textura do sprite
            const sprite = scene.add.sprite(worldX, worldY, object.objectCode);

            // Ajusta o tamanho do sprite para ocupar o espaço de um tile do grid
            sprite.setDisplaySize(GRID_SIZE, GRID_SIZE);

            // Mantém a tua lógica de Y-sorting (profundidade baseada no eixo Y)
            sprite.setDepth(worldY);

            // Define uma profundidade baixa para os objetos do mapa (mais atrás)
            sprite.setDepth(worldY); // Profundidade menor

            // Armazena referência para controle futuro (opcional)
            (sprite as any).objectId = object.id;
        });
    }
}