export interface Position {
    x: number;
    y: number;
}

export interface PlayerData {
    id: number;
    name: string;
    position: Position;
}

export interface AttackData {
    attackerId: string;
    targetId: string;
    damage: number;
}

export interface MapObjectData {
    id: number;
    name: string;
    position: Position;
    type: string;
    isPassable: boolean;
}
