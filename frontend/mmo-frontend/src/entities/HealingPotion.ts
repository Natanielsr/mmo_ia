import Phaser from 'phaser';
import type { Position, ItemData } from '../types';

const GRID_SIZE = 64;

export class HealingPotion extends Phaser.GameObjects.Sprite {
    public id: string;
    public gridPosition: Position;

    constructor(
        itemData: ItemData,
        scene: Phaser.Scene
    ) {
        const worldPos = HealingPotion.getWorldCoordinates(itemData.position);
        super(scene, worldPos.x, worldPos.y, 'potion');

        this.id = itemData.id;
        this.gridPosition = itemData.position;

        this.setDisplaySize(GRID_SIZE * 0.6, GRID_SIZE * 0.6);
        this.setDepth(worldPos.y - 1); // Just below players/monsters depth

        // Add a simple idle animation (bobbing)
        scene.tweens.add({
            targets: this,
            y: this.y - 5,
            duration: 800,
            yoyo: true,
            repeat: -1,
            ease: 'Sine.easeInOut'
        });

        // Add a subtle glow effect (using a tinted copy or just a scale pulse)
        scene.tweens.add({
            targets: this,
            scaleX: this.scaleX * 1.1,
            scaleY: this.scaleY * 1.1,
            duration: 1200,
            yoyo: true,
            repeat: -1,
            ease: 'Sine.easeInOut'
        });

        scene.add.existing(this);
    }

    public static getWorldCoordinates(serverPosition: Position): Position {
        const px = serverPosition.x * GRID_SIZE + (GRID_SIZE / 2);
        const py = -serverPosition.y * GRID_SIZE - (GRID_SIZE / 2);
        return { x: px, y: py };
    }

    public collect(): void {
        // Play a small "collect" animation before destroying
        this.scene.tweens.add({
            targets: this,
            y: this.y - 50,
            alpha: 0,
            scaleX: 1.5,
            scaleY: 1.5,
            duration: 400,
            ease: 'Power2',
            onComplete: () => {
                this.destroy();
            }
        });
    }
}
