import type { ItemData } from '../../types';
import { BaseWorldItem } from './BaseWorldItem';

export class WorldLegs extends BaseWorldItem {
    public defenseBonus: number;

    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, itemData.tagName || 'leather-pants');
        this.defenseBonus = itemData.defenseBonus ?? 0;
    }
}
