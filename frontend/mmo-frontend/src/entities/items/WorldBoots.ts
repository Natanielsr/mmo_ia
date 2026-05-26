import type { ItemData } from '../../types';
import { BaseWorldItem } from './BaseWorldItem';

export class WorldBoots extends BaseWorldItem {
    public defenseBonus: number;

    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, 'iron-boots');
        this.defenseBonus = itemData.defenseBonus ?? 0;
    }
}
