import type { ItemData } from '../../types';
import type { Position } from '../../types';
import { gridToWorld } from '../../utils/coords';
import { BaseWorldItem } from './BaseWorldItem';

export class HealingPotion extends BaseWorldItem {
    constructor(itemData: ItemData, scene: Phaser.Scene) {
        super(itemData, scene, 'potion', { offsetY: -5, duration: 800 });

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

    public static getWorldCoordinates(serverPosition: Position): Position {
        return gridToWorld(serverPosition);
    }
}
