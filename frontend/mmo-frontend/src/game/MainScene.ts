import Phaser from 'phaser';
import { Player } from './Player';
import { Monster } from './Monster';
import { MapLoader } from './MapLoader';
import { DebugPanel } from './DebugPanel';
import type { PlayerPosData, Position, MonsterData } from '../types';

const GRID_SIZE = 64;
const PLAYER_POSITION_OFFSET_X = 0;
const PLAYER_POSITION_OFFSET_Y = 7;

export class MainScene extends Phaser.Scene {
    // Armazenamento de jogadores
    private players: Record<string, Player> = {};
    public myId: string | null = null;

    // Armazenamento de monstros
    private monsters: Record<string, Monster> = {};

    private playerSpeed: number = 2.0;
    private minTimeBetweenMovesMs: number = 1000 / this.playerSpeed;

    private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
    private wasd!: Record<string, Phaser.Input.Keyboard.Key>;
    private debugKey!: Phaser.Input.Keyboard.Key;
    private debugPanel?: DebugPanel;

    public onRequestMove?: (direction: string) => void;
    public onAttackMonster?: (targetId: string) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    public async loadMap() {
        // Carrega os objetos do mapa
        const mapLoader = MapLoader.getInstance();
        await mapLoader.loadMapObjects();
        mapLoader.renderMapObjects(this);
    }

    async create() {
        this.add.grid(0, 0, 2048, 2048, GRID_SIZE, GRID_SIZE, 0x165227, 1, 0xffffff, 0.05).setDepth(-100000);

        this.generateRandomGrass();

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

            this.debugKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.F3);

            if (import.meta.env.DEV) {
                this.debugPanel = new DebugPanel();
            }
        }
    }

    generateRandomGrass() {
        // 1. Array com os nomes das chaves que carregamos ('grass1', 'grass2', etc)
        const grassTypes = Array.from({ length: 12 }, (_, i) => `grass${i + 1}`);

        const mapSize = 2048;
        const halfMap = mapSize / 2; // 1024

        // 2. Loop varrendo do extremo esquerdo (-1024) até o extremo direito (1024)
        for (let x = -halfMap; x < halfMap; x += GRID_SIZE) {

            // Loop varrendo do extremo superior (-1024) até o extremo inferior (1024)
            for (let y = -halfMap; y < halfMap; y += GRID_SIZE) {

                // Escolhe um nome de grama aleatório do array
                const randomGrass = Phaser.Math.RND.pick(grassTypes);

                // Adiciona a imagem. Somamos GRID_SIZE/2 para que o centro da imagem 
                // fique exatamente no meio do "quadrado" do grid.
                this.add.image(x + (GRID_SIZE / 2), y + (GRID_SIZE / 2), randomGrass)
                    .setDisplaySize(GRID_SIZE, GRID_SIZE) // Garante que tem 64x64
                    .setDepth(-100000); // Garante que fica no fundo
            }
        }
    }

    update() {
        if (!this.myId || !this.onRequestMove) return;

        let direction: string | null = null;

        const myPlayer = this.getMyPlayer();

        if (!myPlayer || !myPlayer.isAttacking) {
            if (this.cursors.up.isDown || this.wasd.up.isDown) direction = "north";
            else if (this.cursors.down.isDown || this.wasd.down.isDown) direction = "south";
            else if (this.cursors.left.isDown || this.wasd.left.isDown) direction = "west";
            else if (this.cursors.right.isDown || this.wasd.right.isDown) direction = "east";

            if (direction) {
                this.onRequestMove(direction);
            }

            // Ataque com Barra de Espaço
            if (Phaser.Input.Keyboard.JustDown(this.cursors.space)) {
                console.log("Barra de espaço pressionada!");
                this.attackNearestMonster();
            }
        }

        // Toggle Debug Panel (F3)
        if (Phaser.Input.Keyboard.JustDown(this.debugKey)) {
            this.debugPanel?.toggle();
        }

        // Atualiza Debug Panel
        if (this.debugPanel) {
            this.debugPanel.update(this.monsters);
        }
    }

    private attackNearestMonster(): void {
        const myPlayer = this.getMyPlayer();
        console.log(this.onAttackMonster);
        if (!myPlayer || !this.onAttackMonster) return;

        let nearestMonster: Monster | null = null;

        console.log("--- Iniciando busca por monstro adjacente ---");

        for (const id in this.monsters) {
            const monster = this.monsters[id];
            if (monster.isDead) continue;

            const dx = Math.round(Math.abs(myPlayer.gridPosition.x - monster.gridPosition.x));
            const dy = Math.round(Math.abs(myPlayer.gridPosition.y - monster.gridPosition.y));

            console.log(`Checando monstro ${monster.name}: id=${monster.id}, dist_x=${dx}, dist_y=${dy}`);

            if (dx <= 1 && dy <= 1 && (dx + dy) > 0) { // Adjacente mas não no mesmo tile (se for o caso)
                nearestMonster = monster;
                break; // Ataca o primeiro adjacente encontrado
            }
        }

        if (nearestMonster) {
            console.log(`Atacando monstro: ${nearestMonster.name} (${nearestMonster.id})`);
            this.onAttackMonster(nearestMonster.id);
        } else {
            console.log("Nenhum monstro adjacente encontrado.");
        }
    }

    // Métodos para jogadores
    public updatePlayerPosition(playerData: any, isMe: boolean = false): void {
        const id = playerData.id ?? playerData.Id;
        const pos = playerData.position ?? playerData.Position;
        if (!pos) return;

        const gridPosition = {
            x: pos.x ?? pos.X,
            y: pos.y ?? pos.Y
        };
        const worldPos = this.getWorldCoordinates(gridPosition);

        if (!this.players[id]) {
            this.spawnNewPlayer(id, playerData.name ?? playerData.Name, playerData, worldPos, isMe);
        } else {
            const player = this.players[id];
            player.gridPosition = gridPosition;
            player.move(worldPos, this.minTimeBetweenMovesMs);
        }
    }

    public updatePlayerStatus(statusData: any): void {
        const id = statusData.id ?? statusData.Id;
        if (this.players[id]) {
            const player = this.players[id];
            player.hp = statusData.hp ?? statusData.Hp ?? player.hp;
            player.maxHp = statusData.maxHp ?? statusData.MaxHp ?? player.maxHp;
            player.isDead = statusData.isDead ?? statusData.IsDead ?? player.isDead;
            player.updateHealthBar();
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
        playerPosData: PlayerPosData,
        worldPosition: Position,
        isMe: boolean): void {
        // Instancia o objeto já passando o GRID_SIZE

        const newPlayer = new Player(id, name, playerPosData, worldPosition, this, GRID_SIZE); // Updated
        this.players[id] = newPlayer;

        if (isMe) {
            this.myId = id;
            if (this.input.keyboard) this.input.keyboard.enabled = true;

            // A câmera agora segue o Container inteiro (o Player)
            this.cameras.main.startFollow(newPlayer, true, 0.1, 0.1);
        }
    }

    public getMyPlayer(): Player | null {
        if (!this.myId) return null;
        return this.players[this.myId] || null;
    }

    public getPlayer(id: string | number): Player | null {
        return this.players[String(id)] || null;
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

        // Habilita interação com o mouse
        newMonster.setSize(GRID_SIZE, GRID_SIZE);
        newMonster.setInteractive();
        newMonster.on('pointerdown', () => {
            if (this.onAttackMonster) {
                this.onAttackMonster(newMonster.id);
            }
        });
    }

    public monsterSpawned(monsterData: MonsterData): void {
        this.syncMonster(monsterData);
    }

    public monsterMoved(monsterData: MonsterData): void {
        this.syncMonster(monsterData);
    }

    public monsterDied(monsterId: string): void {
        if (this.monsters[monsterId]) {
            const monster = this.monsters[monsterId];
            monster.die();

            // Remove o monstro após 5 segundos
            this.time.delayedCall(5000, () => {
                if (this.monsters[monsterId]) {
                    this.monsters[monsterId].destroy();
                    delete this.monsters[monsterId];
                }
            });
        }
    }

    public monsterDamaged(data: { id: string; damage: number; currentHp: number }): void {
        if (this.monsters[data.id]) {
            const monster = this.monsters[data.id];
            monster.takeDamage(data.damage);

            // Mostra dano flutuante
            this.showDamageText(data.damage, monster.x, monster.y);
        }
    }

    public playerAttacked(data: { attackerId: string; targetId: string; damage: number }): void {
        const targetId = data.targetId.toString();
        const attackerId = data.attackerId.toString();

        // Faz o atacante rodar animação se for um player
        if (this.players[attackerId]) {
            const attacker = this.players[attackerId];
            // Se o alvo for player
            if (this.players[targetId]) {
                const target = this.players[targetId];
                attacker.playAttackAnimation(target.x, target.y);
            }
            // Se o alvo for monstro
            else if (this.monsters[targetId]) {
                const monster = this.monsters[targetId];
                attacker.playAttackAnimation(monster.x, monster.y);
            }
        }

        if (this.players[targetId]) {
            const player = this.players[targetId];
            player.takeDamage(data.damage);
            this.showDamageText(data.damage, player.x, player.y);
        }
    }

    public playerDied(playerId: number): void {
        const idStr = playerId.toString();
        if (this.players[idStr]) {
            this.players[idStr].die();
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


