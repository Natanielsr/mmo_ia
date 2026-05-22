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
    level: number;
    experience: number;
    attackPower?: number;
    defense?: number;
}

export type EquipmentSlot = 'Weapon' | 'Helmet' | 'Chest' | 'Legs' | 'Boots' | 'Shield';

export interface AttackData {
    attackerId: string;
    attackerName: string;
    targetId: string;
    targetName: string;
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

export interface ItemData {
    id: string;
    name: string;
    position: Position;
    type: string;
    attackBonus?: number;
    defenseBonus?: number;
    slotIndex?: number;
    quantity?: number;
}

export interface ChunkData {
    cx: number;
    cy: number;
    objects: MapObjectData[];
}
