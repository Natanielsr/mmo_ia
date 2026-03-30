// src/managers/InputManager.ts
import Phaser from 'phaser';

export class InputManager {
    private scene: Phaser.Scene;
    private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
    private wasd!: Record<string, Phaser.Input.Keyboard.Key>;
    public debugKey!: Phaser.Input.Keyboard.Key;
    public mapKey!: Phaser.Input.Keyboard.Key;

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public setup(): void {
        if (!this.scene.input.keyboard) return;

        this.scene.input.keyboard.enabled = true;
        this.scene.input.keyboard.removeCapture('W,A,S,D,UP,DOWN,LEFT,RIGHT,SPACE');

        this.cursors = this.scene.input.keyboard.createCursorKeys();
        this.wasd = this.scene.input.keyboard.addKeys({
            up: Phaser.Input.Keyboard.KeyCodes.W,
            down: Phaser.Input.Keyboard.KeyCodes.S,
            left: Phaser.Input.Keyboard.KeyCodes.A,
            right: Phaser.Input.Keyboard.KeyCodes.D
        }) as Record<string, Phaser.Input.Keyboard.Key>;

        this.debugKey = this.scene.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.F3);
        this.mapKey = this.scene.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.M);
    }

    public isMapJustPressed(): boolean {
        return Phaser.Input.Keyboard.JustDown(this.mapKey);
    }

    public getMovementDirection(): string | null {
        if (this.cursors.up.isDown || this.wasd.up.isDown) return "north";
        if (this.cursors.down.isDown || this.wasd.down.isDown) return "south";
        if (this.cursors.left.isDown || this.wasd.left.isDown) return "west";
        if (this.cursors.right.isDown || this.wasd.right.isDown) return "east";
        return null;
    }

    public isAttackJustPressed(): boolean {
        return Phaser.Input.Keyboard.JustDown(this.cursors.space);
    }

    public isDebugJustPressed(): boolean {
        return Phaser.Input.Keyboard.JustDown(this.debugKey);
    }
}