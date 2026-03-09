import Phaser from 'phaser';
import { Player } from './Player'; // Importe a nova classe aqui!
import type { PlayerData, Position } from '../types';

const GRID_SIZE = 64;
const PLAYER_POSITION_OFFSET_X = 0;
const PLAYER_POSITION_OFFSET_Y = 15;

export class MainScene extends Phaser.Scene {
    // Agora armazenamos instâncias da classe Player diretamente
    private players: Record<string, Player> = {};
    public myId: number | null = null;

    private playerSpeed: number = 4.0;
    private minTimeBetweenMovesMs: number = 1000 / this.playerSpeed;

    private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
    private wasd!: Record<string, Phaser.Input.Keyboard.Key>;

    public onRequestMove?: (direction: string) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    preload() {
        this.load.spritesheet('hero', 'assets/human_base_16x18.png', {
            frameWidth: 16,
            frameHeight: 18
        });
    }

    create() {
        this.add.grid(0, 0, 2048, 2048, GRID_SIZE, GRID_SIZE, 0x0f172a, 1, 0xffffff, 0.05);

        // Criação das animações mantém-se aqui, pois são recursos globais da Cena
        this.anims.create({ key: 'walk-north', frames: this.anims.generateFrameNumbers('hero', { start: 0, end: 2 }), frameRate: 16, repeat: -1 });
        this.anims.create({ key: 'walk-east', frames: this.anims.generateFrameNumbers('hero', { start: 9, end: 11 }), frameRate: 16, repeat: -1 });
        this.anims.create({ key: 'walk-south', frames: this.anims.generateFrameNumbers('hero', { start: 18, end: 20 }), frameRate: 16, repeat: -1 });
        this.anims.create({ key: 'walk-west', frames: this.anims.generateFrameNumbers('hero', { start: 27, end: 29 }), frameRate: 16, repeat: -1 });

        if (this.input.keyboard) {
            this.input.keyboard.enabled = false;
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

    update() {
        if (!this.myId || !this.onRequestMove) return;

        let direction: string | null = null;

        if (this.cursors.up.isDown || this.wasd.up.isDown) direction = "north";
        else if (this.cursors.down.isDown || this.wasd.down.isDown) direction = "south";
        else if (this.cursors.left.isDown || this.wasd.left.isDown) direction = "west";
        else if (this.cursors.right.isDown || this.wasd.right.isDown) direction = "east";

        if (direction) {
            this.onRequestMove(direction);
        }
    }

    public updatePlayerPosition(playerData: PlayerData, isMe: boolean = false): void {
        const serverPosition = playerData.position;
        const worldPos = this.getWorldCoordinates(serverPosition);

        if (!this.players[playerData.id]) {
            this.spawnNewPlayer(playerData.id, playerData.name, serverPosition, worldPos, isMe);
        } else {
            // Delega o movimento e a animação totalmente para a classe do jogador
            this.players[playerData.id].serverPosition = serverPosition;
            this.players[playerData.id].move(worldPos, this.minTimeBetweenMovesMs);
        }
    }

    private getWorldCoordinates(serverPosition: Position): Position {
        const px = (serverPosition.x * GRID_SIZE + (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_X;
        const py = (-serverPosition.y * GRID_SIZE - (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_Y;
        return { x: px, y: py };
    }

    private spawnNewPlayer(
        id: number,
        name: string,
        serverPosition: Position,
        worldPosition: Position,
        isMe: boolean): void {
        // Instancia o objeto já passando o GRID_SIZE

        const newPlayer = new Player(id, name, serverPosition, worldPosition, this, GRID_SIZE);
        this.players[id] = newPlayer;

        if (isMe) {
            this.myId = id;
            if (this.input.keyboard) this.input.keyboard.enabled = true;

            // A câmera agora segue o Container inteiro (o Player)
            this.cameras.main.startFollow(newPlayer, true, 0.1, 0.1);
        }
    }

    public removePlayer(id: string) {
        if (this.players[id]) {
            // Chama o método destroy() nativo do Phaser que nós sobrescrevemos na classe
            this.players[id].destroy();
            delete this.players[id];
        }
    }
}