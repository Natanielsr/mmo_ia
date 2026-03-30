// src/scenes/MainScene.ts
import Phaser from 'phaser';
import { DebugPanel } from './DebugPanel';
import { InputManager } from '../managers/InputManager';
import { PlayerManager } from '../managers/PlayerManager';
import { MonsterManager } from '../managers/MonsterManager';
import { ItemManager } from '../managers/ItemManager';
import { ChunkManager } from '../managers/ChunkManager';
import { CombatSystem } from '../systems/CombatSystem';
import type { MonsterData } from '../types';
import type { Player } from '../entities/Player';
import type { Monster } from '../entities/Monster';
import { updateUIPosition } from '../ui';

export class MainScene extends Phaser.Scene {
    public playerManager!: PlayerManager;
    public monsterManager!: MonsterManager;
    public itemManager!: ItemManager;
    public chunkManager!: ChunkManager;
    private inputManager!: InputManager;
    private combatSystem!: CombatSystem;
    private debugPanel?: DebugPanel;

    public onRequestMove?: (direction: string) => void;
    public onAttackMonster?: (targetId: string) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    public async loadMap() {
        // Obsoleto: O mapa agora é carregado via Chunks pelo SignalR
    }

    async create() {
        // Inicializa os Managers
        this.inputManager = new InputManager(this);
        this.playerManager = new PlayerManager(this);
        this.monsterManager = new MonsterManager(this);
        this.itemManager = new ItemManager(this);
        this.chunkManager = new ChunkManager(this);
        this.combatSystem = new CombatSystem(this, this.playerManager, this.monsterManager);

        this.inputManager.setup();

        // Renderização de Fundo: O fundo agora é gerencia pelos Chunks.
        // Removemos o grid gigante que também consumia recursos desnecessários.

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

        if (myPlayer) {
            // Otimização: Atualiza o gerenciador de chunks para limpar o que está longe
            // O loadRadius aqui no frontend deve ser compatível com o do backend (WorldConfig)
            this.chunkManager.update(myPlayer.gridPosition, 1);

            // Atualiza UI com a posição
            updateUIPosition(myPlayer.gridPosition.x, myPlayer.gridPosition.y);

            if (!myPlayer.isAttacking) {
                const direction = this.inputManager.getMovementDirection();
                if (direction) {
                    this.onRequestMove(direction);
                }

                if (this.inputManager.isAttackJustPressed()) {
                    this.combatSystem.attackNearestMonster(this.onAttackMonster);
                }
            }
        }

        if (this.inputManager.isDebugJustPressed()) {
            this.debugPanel?.toggle();
        }

        if (this.debugPanel) {
            this.debugPanel.update(this.monsterManager.getMonstersDict());
        }
    }

    // --- Chunk Logic ---
    public chunkLoaded(data: any) {
        this.chunkManager.handleChunkLoaded(data);
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
    public monsterRemoved(id: string) { this.monsterManager.removeMonster(id); }
    public getMonster(id: string): Monster | null { return this.monsterManager.getMonster(id) };

    public monsterDamaged(data: any) { this.combatSystem.handleMonsterDamaged(data); }
    public playerAttacked(data: any) { this.combatSystem.handlePlayerAttacked(data); }

    public itemDropped(data: any) { this.itemManager.syncItem(data); }
    public itemPickedUp(data: any) { this.itemManager.removeItem(data.itemId); }
}