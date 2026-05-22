import Phaser from 'phaser';
import type { Position, ItemData } from '../types';
import { GRID_SIZE } from '../config/constants';
import { gridToWorld } from '../utils/coords';
import type { IWorldItem } from './IWorldItem';

export class WorldShield extends Phaser.GameObjects.Sprite implements IWorldItem {
    public id: string;
    public gridPosition: Position;
    public defenseBonus: number;

    constructor(itemData: ItemData, scene: Phaser.Scene) {
        const worldPos = gridToWorld(itemData.position);
        super(scene, worldPos.x, worldPos.y, 'wooden-shield');

        this.id = itemData.id;
        this.gridPosition = itemData.position;
        this.defenseBonus = itemData.defenseBonus ?? 0;

        this.setDisplaySize(GRID_SIZE * 0.5, GRID_SIZE * 0.5);
        this.setDepth(worldPos.y - 1);

        scene.tweens.add({
            targets: this,
            y: this.y - 4,
            duration: 950,
            yoyo: true,
            repeat: -1,
            ease: 'Sine.easeInOut',
        });

        scene.add.existing(this);
    }

    public collect(): void {
        this.scene.tweens.add({
            targets: this,
            y: this.y - 50,
            alpha: 0,
            scaleX: 1.5,
            scaleY: 1.5,
            duration: 400,
            ease: 'Power2',
            onComplete: () => { this.destroy(); },
        });
    }
}
