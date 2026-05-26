import Phaser from 'phaser';
import type { Position, ItemData } from '../../types';
import { WORLD_ITEM_SIZE } from '../../config/constants';
import { gridToWorld } from '../../utils/coords';
import type { IWorldItem } from './IWorldItem';

interface BobConfig {
    offsetY: number;
    duration: number;
}

const DEFAULT_BOB: BobConfig = { offsetY: -4, duration: 950 };

export abstract class BaseWorldItem extends Phaser.GameObjects.Sprite implements IWorldItem {
    public id: string;
    public gridPosition: Position;

    constructor(
        itemData: ItemData,
        scene: Phaser.Scene,
        textureKey: string,
        bobConfig: BobConfig = DEFAULT_BOB,
    ) {
        const worldPos = gridToWorld(itemData.position);
        super(scene, worldPos.x, worldPos.y, textureKey);

        this.id = itemData.id;
        this.gridPosition = itemData.position;

        this.setDisplaySize(WORLD_ITEM_SIZE, WORLD_ITEM_SIZE);
        this.setDepth(worldPos.y + 1);

        scene.tweens.add({
            targets: this,
            y: this.y + bobConfig.offsetY,
            duration: bobConfig.duration,
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
