export interface Position {
    x: number;
    y: number;
}

export interface PlayerData {
    id: string;
    name: string;
    position: Position;
}

export interface AttackData {
    attackerId: string;
    targetId: string;
    damage: number;
}