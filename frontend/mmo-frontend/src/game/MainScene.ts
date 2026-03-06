import Phaser from 'phaser';

const GRID_SIZE = 32;

// Interface para guardar a referência visual do jogador
interface PlayerSprite {
    sprite: Phaser.GameObjects.Sprite;
    nameText: Phaser.GameObjects.Text;
}

export class MainScene extends Phaser.Scene {
    private playerSprites: Record<string, PlayerSprite> = {};
    public myId: string | null = null;

    private lastMoveTime: number = 0;
    private playerSpeed: number = 4.0;
    private minTimeBetweenMovesMs: number = 1000 / this.playerSpeed;

    private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
    private wasd!: Record<string, Phaser.Input.Keyboard.Key>;

    // Callback injetado pelo main.ts para enviar comandos para o SignalR
    public onRequestMove?: (direction: string) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    preload() {
        this.load.image('token', 'https://labs.phaser.io/assets/sprites/aqua_ball.png');
    }

    create() {
        // Grelha visual
        this.add.grid(0, 0, 2048, 2048, GRID_SIZE, GRID_SIZE, 0x0f172a, 1, 0xffffff, 0.05);

        if (this.input.keyboard) {
            this.input.keyboard.enabled = false;
            // Remove global capture para não interferir com o browser
            this.input.keyboard.removeCapture('W,A,S,D,UP,DOWN,LEFT,RIGHT,SPACE');

            this.cursors = this.input.keyboard.createCursorKeys();
            this.wasd = this.input.keyboard.addKeys({
                up: Phaser.Input.Keyboard.KeyCodes.W,
                down: Phaser.Input.Keyboard.KeyCodes.S,
                left: Phaser.Input.Keyboard.KeyCodes.A,
                right: Phaser.Input.Keyboard.KeyCodes.D
            }) as Record<string, Phaser.Input.Keyboard.Key>;
        }
    }

    update(time: number) {
        if (!this.myId || !this.onRequestMove) return;

        let direction: string | null = null;

        if (this.cursors.up.isDown || this.wasd.up.isDown) direction = "north";
        else if (this.cursors.down.isDown || this.wasd.down.isDown) direction = "south";
        else if (this.cursors.left.isDown || this.wasd.left.isDown) direction = "west";
        else if (this.cursors.right.isDown || this.wasd.right.isDown) direction = "east";

        if (direction && time - this.lastMoveTime >= this.minTimeBetweenMovesMs) {
            this.onRequestMove(direction);
            this.lastMoveTime = time;
        }
    }

    public updatePlayerPosition(id: string, gridX: number, gridY: number, isMe: boolean = false) {
        const px = gridX * GRID_SIZE + (GRID_SIZE / 2);
        const py = -gridY * GRID_SIZE - (GRID_SIZE / 2);

        if (!this.playerSprites[id]) {
            const sprite = this.add.sprite(px, py, 'token');
            const nameText = this.add.text(px, py - 20, id, {
                fontSize: '12px', color: '#fff', fontFamily: 'Inter'
            }).setOrigin(0.5);

            this.playerSprites[id] = { sprite, nameText };

            if (isMe) {
                this.myId = id;
                if (this.input.keyboard) this.input.keyboard.enabled = true; // Reativa o teclado para movimento
                sprite.setTint(0x818cf8);
                this.cameras.main.startFollow(sprite, true, 0.1, 0.1);
            } else {
                sprite.setTint(0xef4444);
            }
        } else {
            const p = this.playerSprites[id];

            this.tweens.add({
                targets: [p.sprite, p.nameText],
                x: px,
                y: (target: any) => target.type === 'Text' ? py - 20 : py,
                duration: this.minTimeBetweenMovesMs - 50,
                ease: 'Linear'
            });
        }
    }

    public removePlayer(id: string) {
        if (this.playerSprites[id]) {
            this.playerSprites[id].sprite.destroy();
            this.playerSprites[id].nameText.destroy();
            delete this.playerSprites[id];
        }
    }
}