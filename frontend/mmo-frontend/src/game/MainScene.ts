import Phaser from 'phaser';

const GRID_SIZE = 64;
const PLAYER_POSITION_OFFSET_X = 0;
const PLAYER_POSITION_OFFSET_Y = 15;

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
        // Carrega o spritesheet (1024x1024 total, 4x4 frames de 256x256 cada)
        this.load.spritesheet('hero', 'assets/16x16.png', {
            frameWidth: 16,
            frameHeight: 18
        });
    }

    create() {
        // Grelha visual
        this.add.grid(0, 0, 2048, 2048, GRID_SIZE, GRID_SIZE, 0x0f172a, 1, 0xffffff, 0.05);

        // Define animações correspondentes às linhas do spritesheet
        this.anims.create({
            key: 'walk-north',
            frames: this.anims.generateFrameNumbers('hero', { start: 0, end: 2 }),
            frameRate: 16,
            repeat: -1
        });
        this.anims.create({
            key: 'walk-east',
            frames: this.anims.generateFrameNumbers('hero', { start: 3, end: 5 }),
            frameRate: 16,
            repeat: -1
        });
        this.anims.create({
            key: 'walk-south',
            frames: this.anims.generateFrameNumbers('hero', { start: 6, end: 8 }),
            frameRate: 16,
            repeat: -1
        });
        this.anims.create({
            key: 'walk-west',
            frames: this.anims.generateFrameNumbers('hero', { start: 9, end: 11 }),
            frameRate: 16,
            repeat: -1
        });

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
        const px = (gridX * GRID_SIZE + (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_X;
        const py = (-gridY * GRID_SIZE - (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_Y;

        if (!this.playerSprites[id]) {
            const sprite = this.add.sprite(px, py, 'hero', 0);
            sprite.setDisplaySize(GRID_SIZE * 1, GRID_SIZE * 1); // Aumentado para o novo spritesheet

            const nameText = this.add.text(px, py - (GRID_SIZE / 2 + 10), id, {
                fontSize: '14px', color: '#fff', fontFamily: 'Inter'
            }).setOrigin(0.5);

            this.playerSprites[id] = { sprite, nameText };
            const p = this.playerSprites[id];
            const startAnim = this.anims.get('walk-south');
            const startFrame = startAnim.frames[1].textureFrame;
            p.sprite.setFrame(startFrame);

            if (isMe) {
                this.myId = id;
                if (this.input.keyboard) this.input.keyboard.enabled = true;
                this.cameras.main.startFollow(sprite, true, 0.1, 0.1);
            }
        } else {
            const p = this.playerSprites[id];

            // Detecta direção para animação
            let animKey = '';
            const dx = px - p.sprite.x;
            const dy = py - p.sprite.y;

            // Só calcula a direção se houver algum movimento
            if (dx !== 0 || dy !== 0) {
                // Avalia qual eixo tem o maior deslocamento absoluto
                if (Math.abs(dx) > Math.abs(dy)) {
                    // Movimento predominantemente horizontal
                    animKey = dx > 0 ? 'walk-east' : 'walk-west';
                } else {
                    // Movimento predominantemente vertical (ou diagonal exata)
                    animKey = dy > 0 ? 'walk-south' : 'walk-north';
                }
            }

            if (animKey) {
                p.sprite.play(animKey, true);
            }

            // Remove tweens ativos para evitar conflitos
            this.tweens.killTweensOf([p.sprite, p.nameText]);

            this.tweens.add({
                targets: [p.sprite, p.nameText],
                x: px,
                y: (target: any) => target.type === 'Text' ? py - (GRID_SIZE / 2 + 10) : py,
                duration: this.minTimeBetweenMovesMs,
                ease: 'Linear',
                onComplete: () => {
                    p.sprite.stop(); // Para a animação ao chegar no tile

                    // Verifica a animação atual e pega o 2º frame (índice 1) dela
                    if (p.sprite.anims.currentAnim) {
                        // frames[1] pega o segundo quadro da sequência atual
                        const frameParado = p.sprite.anims.currentAnim.frames[1].textureFrame;
                        p.sprite.setFrame(frameParado);
                    }
                }
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