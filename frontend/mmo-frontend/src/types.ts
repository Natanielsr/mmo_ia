export interface Position {
    x: number;
    y: number;
}

export interface PlayerData {
    playerId: string;
    name?: string;
    position?: Position;
    x?: number;
    y?: number;
}

export interface AttackData {
    attackerId: string;
    targetId: string;
    damage: number;
}