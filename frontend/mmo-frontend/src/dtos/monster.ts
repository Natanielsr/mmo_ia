export interface MonsterData {
    id: string;
    name: string;
    objectCode: string;
    position: import('./position').Position;
    hp: number;
    maxHp: number;
    attackPower: number;
    isDead: boolean;
}
