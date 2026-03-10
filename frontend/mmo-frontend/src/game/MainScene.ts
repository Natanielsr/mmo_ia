import Phaser from 'phaser';
import { Player } from './Player';
import { Monster } from './Monster';
import { MapLoader } from './MapLoader';
import type { PlayerData, Position, MonsterData } from '../types';

const GRID_SIZE = 64;
const PLAYER_POSITION_OFFSET_X = 0;
const PLAYER_POSITION_OFFSET_Y = 15;

export class MainScene extends Phaser.Scene {
    // Armazenamento de jogadores
    private players: Record<string, Player> = {};
    public myId: string | null = null;

    // Armazenamento de monstros
    private monsters: Record<string, Monster> = {};

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

    public async loadMap() {
        // Carrega os objetos do mapa
        const mapLoader = MapLoader.getInstance();
        await mapLoader.loadMapObjects();
        mapLoader.renderMapObjects(this);
    }

    async create() {
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

    // Métodos para jogadores
    public updatePlayerPosition(playerData: PlayerData, isMe: boolean = false): void {
        const gridPosition = playerData.position; // Updated variable name
        const worldPos = this.getWorldCoordinates(gridPosition);

        if (!this.players[playerData.id]) {
            this.spawnNewPlayer(playerData.id, playerData.name, gridPosition, worldPos, isMe);
        } else {
            // Delega o movimento e a animação totalmente para a classe do jogador
            this.players[playerData.id].gridPosition = gridPosition; // Updated property name
            this.players[playerData.id].move(worldPos, this.minTimeBetweenMovesMs);
        }
    }

    private getWorldCoordinates(gridPosition: Position): Position { // Updated parameter name
        const px = (gridPosition.x * GRID_SIZE + (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_X;
        const py = (-gridPosition.y * GRID_SIZE - (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_Y;
        return { x: px, y: py };
    }

    private spawnNewPlayer(
        id: string,
        name: string,
        gridPosition: Position, // Updated parameter name
        worldPosition: Position,
        isMe: boolean): void {
        // Instancia o objeto já passando o GRID_SIZE

        const newPlayer = new Player(id, name, gridPosition, worldPosition, this, GRID_SIZE); // Updated
        this.players[id] = newPlayer;

        if (isMe) {
            this.myId = id;
            if (this.input.keyboard) this.input.keyboard.enabled = true;

            // A câmera agora segue o Container inteiro (o Player)
            this.cameras.main.startFollow(newPlayer, true, 0.1, 0.1);
        }
    }

    public removePlayer(id: number) {
        if (this.players[id]) {
            // Chama o método destroy() nativo do Phaser que nós sobrescrevemos na classe
            this.players[id].destroy();
            delete this.players[id];
        }
    }

    // Métodos para monstros
    public syncMonsters(monsterList: MonsterData[]): void {
        // Processa todos os monstros recebidos do servidor
        monsterList.forEach(monsterData => {
            this.syncMonster(monsterData);
        });

        console.log(`Sincronizados ${monsterList.length} monstros`);
    }

    public syncMonster(monsterData: MonsterData): void {
        if (this.monsters[monsterData.id]) {
            // Atualiza monstro existente
            this.monsters[monsterData.id].updateFromServer(monsterData);
        } else {
            // Cria novo monstro
            this.spawnMonster(monsterData);
        }
    }

    private spawnMonster(monsterData: MonsterData): void {
        const newMonster = new Monster(monsterData, this);
        this.monsters[monsterData.id] = newMonster;
        console.log(`Monstro spawnado: ${monsterData.name} (ID: ${monsterData.id})`);
    }

    public monsterSpawned(monsterData: MonsterData): void {
        this.syncMonster(monsterData);
    }

    public monsterMoved(monsterData: MonsterData): void {
        this.syncMonster(monsterData);
    }

    public monsterDied(monsterId: number): void {
        if (this.monsters[monsterId]) {
            const monster = this.monsters[monsterId];
            monster.isDead = true;
            monster.sprite.setAlpha(0.5);
            monster.nameText.setText(`${monster.name} 💀`);

            // Remove o monstro após 5 segundos
            this.time.delayedCall(5000, () => {
                if (this.monsters[monsterId]) {
                    this.monsters[monsterId].destroy();
                    delete this.monsters[monsterId];
                }
            });
        }
    }

    public monsterDamaged(data: { monsterId: number; damage: number; currentHp: number }): void {
        if (this.monsters[data.monsterId]) {
            const monster = this.monsters[data.monsterId];
            monster.takeDamage(data.damage);

            // Mostra dano flutuante
            this.showDamageText(data.damage, monster.x, monster.y);
        }
    }

    private showDamageText(damage: number, x: number, y: number): void {
        const damageText = this.add.text(x, y - 50, `-${damage}`, {
            fontSize: '16px',
            color: '#FF5555',
            fontFamily: 'Inter',
            stroke: '#000000',
            strokeThickness: 3
        }).setOrigin(0.5).setDepth(10000);

        // Animação flutuante
        this.tweens.add({
            targets: damageText,
            y: y - 100,
            alpha: 0,
            duration: 1000,
            ease: 'Power2',
            onComplete: () => {
                damageText.destroy();
            }
        });
    }
}