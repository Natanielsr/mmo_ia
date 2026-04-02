// src/managers/InputManager.ts
import Phaser from 'phaser';

export class InputManager {
    private scene: Phaser.Scene;
    private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
    private wasd!: Record<string, Phaser.Input.Keyboard.Key>;
    public debugKey!: Phaser.Input.Keyboard.Key;
    public mapKey!: Phaser.Input.Keyboard.Key;

    private touchDirection: string | null = null;
    private touchAttackPressed: boolean = false;

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

        this.setupTouchControls();
    }

    private setupTouchControls(): void {
        const btnUp = document.getElementById('btn-up');
        const btnDown = document.getElementById('btn-down');
        const btnLeft = document.getElementById('btn-left');
        const btnRight = document.getElementById('btn-right');
        const btnAttack = document.getElementById('btn-attack');

        const bindDirection = (btn: HTMLElement | null, direction: string) => {
            if (!btn) return;
            // Touch Events
            btn.addEventListener('touchstart', (e) => { e.preventDefault(); this.touchDirection = direction; btn.classList.add('active'); }, { passive: false });
            btn.addEventListener('touchend', (e) => { e.preventDefault(); if (this.touchDirection === direction) this.touchDirection = null; btn.classList.remove('active'); }, { passive: false });
            btn.addEventListener('touchcancel', (e) => { e.preventDefault(); if (this.touchDirection === direction) this.touchDirection = null; btn.classList.remove('active'); }, { passive: false });
            
            // Mouse Events for testing
            btn.addEventListener('mousedown', (e) => { e.preventDefault(); this.touchDirection = direction; btn.classList.add('active'); });
            btn.addEventListener('mouseup', (e) => { e.preventDefault(); if (this.touchDirection === direction) this.touchDirection = null; btn.classList.remove('active'); });
            btn.addEventListener('mouseleave', (e) => { e.preventDefault(); if (this.touchDirection === direction) this.touchDirection = null; btn.classList.remove('active'); });
        };

        bindDirection(btnUp, 'north');
        bindDirection(btnDown, 'south');
        bindDirection(btnLeft, 'west');
        bindDirection(btnRight, 'east');

        if (btnAttack) {
            btnAttack.addEventListener('touchstart', (e) => { e.preventDefault(); this.touchAttackPressed = true; btnAttack.classList.add('active'); }, { passive: false });
            btnAttack.addEventListener('touchend', (e) => { e.preventDefault(); btnAttack.classList.remove('active'); }, { passive: false });
            btnAttack.addEventListener('touchcancel', (e) => { e.preventDefault(); btnAttack.classList.remove('active'); }, { passive: false });
            
            btnAttack.addEventListener('mousedown', (e) => { e.preventDefault(); this.touchAttackPressed = true; btnAttack.classList.add('active'); });
            btnAttack.addEventListener('mouseup', (e) => { e.preventDefault(); btnAttack.classList.remove('active'); });
            btnAttack.addEventListener('mouseleave', (e) => { e.preventDefault(); btnAttack.classList.remove('active'); });
        }
    }

    public isMapJustPressed(): boolean {
        return Phaser.Input.Keyboard.JustDown(this.mapKey);
    }

    public getMovementDirection(): string | null {
        if (this.touchDirection) return this.touchDirection;
        if (this.cursors.up.isDown || this.wasd.up.isDown) return "north";
        if (this.cursors.down.isDown || this.wasd.down.isDown) return "south";
        if (this.cursors.left.isDown || this.wasd.left.isDown) return "west";
        if (this.cursors.right.isDown || this.wasd.right.isDown) return "east";
        return null;
    }

    public isAttackJustPressed(): boolean {
        if (this.touchAttackPressed) {
            this.touchAttackPressed = false;
            return true;
        }
        return Phaser.Input.Keyboard.JustDown(this.cursors.space);
    }

    public isDebugJustPressed(): boolean {
        return Phaser.Input.Keyboard.JustDown(this.debugKey);
    }
}