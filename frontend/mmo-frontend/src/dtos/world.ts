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
}

export interface WorldMapData {
    cols: number;
    rows: number;
    tiles: number[][];   // tiles[row][col] = TileType ordinal
    biomes: number[][];  // biomes[row][col] = 0=Grass 1=Sand 2=Snow
}
