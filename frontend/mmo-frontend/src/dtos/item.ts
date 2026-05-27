export type EquipmentSlot = 'Weapon' | 'Helmet' | 'Chest' | 'Legs' | 'Boots' | 'Shield';

export interface ItemData {
    id: string;
    name: string;
    description?: string;
    value?: number;
    tagName: string;
    position: import('./position').Position;
    type: string;
    attackBonus?: number;
    defenseBonus?: number;
    slotIndex?: number;
    quantity?: number;
}
