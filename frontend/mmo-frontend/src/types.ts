export interface Position {
    x: number;
    y: number;
}

export interface PlayerPosData {
    id: string;
    name: string;
    position: Position;
}

export interface PlayerStatusData {
    id: string;
    hp: number;
    maxHp: number;
    isDead: boolean;
}

export interface AttackData {
    attackerId: string;
    targetId: string;
    damage: number;
}

export interface MapObjectData {
    id: string;
    name: string;
    objectCode: string;
    position: Position;
    type: string;
    isPassable: boolean;
}

export interface MonsterData {
    id: string;
    name: string;
    objectCode: string;
    position: Position;
    hp: number;
    maxHp: number;
    attackPower: number;
    isDead: boolean;
}
