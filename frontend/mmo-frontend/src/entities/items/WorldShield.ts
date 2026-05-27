import type { ItemData } from '../../types';
import { BaseWorldItem } from './BaseWorldItem';

export class WorldShield extends BaseWorldItem {
    public defenseBonus: number;

    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, itemData.tagName || 'wooden-shield');
        this.defenseBonus = itemData.defenseBonus ?? 0;
    }
}
