export interface MapObjectData {
    id: string;
    name: string;
    objectCode: string;
    position: import('./position').Position;
    type: string;
    isPassable: boolean;
}

export interface ChunkData {
    cx: number;
    cy: number;
    objects: MapObjectData[];
    items?: import('./item').ItemData[];
    tiles?: number[][];
    biomes?: number[][];
}
