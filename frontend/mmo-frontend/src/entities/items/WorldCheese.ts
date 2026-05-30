import type { ItemData } from '../../types';
import { BaseWorldItem } from './BaseWorldItem';

export class WorldCheese extends BaseWorldItem {
    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, itemData.tagName || 'cheese', { offsetY: -5, duration: 800 });

        scene.tweens.add({
            targets: this,
            scaleX: this.scaleX * 1.1,
            scaleY: this.scaleY * 1.1,
            duration: 1200,
            yoyo: true,
            repeat: -1,
            ease: 'Sine.easeInOut',
        });
    }

    public override collect(): void {
        this.destroy();
    }
}
