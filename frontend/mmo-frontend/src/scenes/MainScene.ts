// src/scenes/MainScene.ts
import Phaser from 'phaser';
import { MapLoader } from './MapLoader';
import { DebugPanel } from './DebugPanel';
import { InputManager } from '../managers/InputManager';
import { PlayerManager } from '../managers/PlayerManager';
import { MonsterManager } from '../managers/MonsterManager';
import { CombatSystem } from '../systems/CombatSystem';
import { GRID_SIZE } from '../config/constants';
import type { MonsterData } from '../types';
import type { Player } from '../entities/Player';
import type { Monster } from '../entities/Monster';

export class MainScene extends Phaser.Scene {
    public playerManager!: PlayerManager;
    public monsterManager!: MonsterManager;
    private inputManager!: InputManager;
    private combatSystem!: CombatSystem;
    private debugPanel?: DebugPanel;

    public onRequestMove?: (direction: string) => void;
    public onAttackMonster?: (targetId: string) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    public async loadMap() {
        const mapLoader = MapLoader.getInstance();
        await mapLoader.loadMapObjects();
        mapLoader.renderMapObjects(this);
    }

    async create() {
        // Inicializa os Managers
        this.inputManager = new InputManager(this);
        this.playerManager = new PlayerManager(this);
        this.monsterManager = new MonsterManager(this);
        this.combatSystem = new CombatSystem(this, this.playerManager, this.monsterManager);

        this.inputManager.setup();

        // Renderização de Fundo
        this.add.grid(0, 0, 2048, 2048, GRID_SIZE, GRID_SIZE, 0x165227, 1, 0xffffff, 0.05).setDepth(-100000);
        this.generateRandomGrass();

        if (import.meta.env.DEV) {
            this.debugPanel = new DebugPanel();
        }

        // Linka os callbacks
        this.monsterManager.onAttackMonster = (id) => {
            if (this.onAttackMonster) this.onAttackMonster(id);
        };
    }

    update() {
        if (!this.playerManager.myId || !this.onRequestMove) return;

        const myPlayer = this.playerManager.getMyPlayer();

        if (!myPlayer || !myPlayer.isAttacking) {
            const direction = this.inputManager.getMovementDirection();
            if (direction) {
                this.onRequestMove(direction);
            }

            if (this.inputManager.isAttackJustPressed()) {
                this.combatSystem.attackNearestMonster(this.onAttackMonster);
            }
        }

        if (this.inputManager.isDebugJustPressed()) {
            this.debugPanel?.toggle();
        }

        if (this.debugPanel) {
            this.debugPanel.update(this.monsterManager.getMonstersDict());
        }
    }

    private generateRandomGrass() {
        const grassTypes = Array.from({ length: 12 }, (_, i) => `grass${i + 1}`);
        const halfMap = 2048 / 2;

        for (let x = -halfMap; x < halfMap; x += GRID_SIZE) {
            for (let y = -halfMap; y < halfMap; y += GRID_SIZE) {
                const randomGrass = Phaser.Math.RND.pick(grassTypes);
                this.add.image(x + (GRID_SIZE / 2), y + (GRID_SIZE / 2), randomGrass)
                    .setDisplaySize(GRID_SIZE, GRID_SIZE)
                    .setDepth(-100000);
            }
        }
    }

    // --- Wrapper Methods para manter a interface com quem chama a cena de fora (ex: Socket.io) ---
    public updatePlayerPosition(data: any, isMe: boolean = false) { this.playerManager.updatePlayerPosition(data, isMe); }
    public updatePlayerStatus(data: any) { this.playerManager.updatePlayerStatus(data); }
    public removePlayer(id: string) { this.playerManager.removePlayer(id); }
    public playerDied(id: string) { this.playerManager.getPlayer(id)?.die(); }
    public getMyPlayer(): Player | null { return this.playerManager.getMyPlayer() }
    public getPlayer(id: string): Player | null { return this.playerManager.getPlayer(id) };

    public syncMonsters(data: MonsterData[]) { this.monsterManager.syncMonsters(data); }
    public monsterSpawned(data: MonsterData) { this.monsterManager.syncMonster(data); }
    public monsterMoved(data: MonsterData) { this.monsterManager.syncMonster(data); }
    public monsterDied(id: string) { this.monsterManager.monsterDied(id); }
    public getMonster(id: string): Monster | null { return this.monsterManager.getMonster(id) };

    public monsterDamaged(data: any) { this.combatSystem.handleMonsterDamaged(data); }
    public playerAttacked(data: any) { this.combatSystem.handlePlayerAttacked(data); }
}