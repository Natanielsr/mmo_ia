import type { ItemData } from '../../types';
import { BaseWorldItem } from './BaseWorldItem';

export class WorldWeapon extends BaseWorldItem {
    public attackBonus: number;

    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, itemData.tagName || 'dagger');
        this.attackBonus = itemData.attackBonus ?? 0;
    }
}
