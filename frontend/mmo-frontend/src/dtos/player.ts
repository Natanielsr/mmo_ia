export interface PlayerData {
    id: string;
    name: string;
    position: import('./position').Position;
    equippedWeaponName?: string | null;
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
