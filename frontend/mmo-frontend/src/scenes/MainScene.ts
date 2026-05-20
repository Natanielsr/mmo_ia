// src/scenes/MainScene.ts
import Phaser from 'phaser';
import { DebugPanel } from './DebugPanel';
import { InputManager } from '../managers/InputManager';
import { PlayerManager } from '../managers/PlayerManager';
import { MonsterManager } from '../managers/MonsterManager';
import { ItemManager } from '../managers/ItemManager';
import { ChunkManager } from '../managers/ChunkManager';
import { InventoryManager } from '../managers/InventoryManager';
import { EquipmentManager } from '../managers/EquipmentManager';
import { InventoryUI } from '../ui/InventoryUI';
import { EquipmentUI } from '../ui/EquipmentUI';
import { DragDropHandler } from '../ui/DragDropHandler';
import { CombatSystem } from '../systems/CombatSystem';
import type { MonsterData, ItemData, EquipmentSlot } from '../types';
import type { Player } from '../entities/Player';
import type { Monster } from '../entities/Monster';
import { updateUIPosition } from '../ui';

export class MainScene extends Phaser.Scene {
    public playerManager!: PlayerManager;
    public monsterManager!: MonsterManager;
    public itemManager!: ItemManager;
    public chunkManager!: ChunkManager;
    public inventoryManager!: InventoryManager;
    public equipmentManager!: EquipmentManager;
    private inputManager!: InputManager;
    private combatSystem!: CombatSystem;
    private debugPanel?: DebugPanel;
    private inventoryUI!: InventoryUI;
    private equipmentUI!: EquipmentUI;
    private dragDropHandler!: DragDropHandler;

    public onRequestMove?: (direction: string) => void;
    public onAttackMonster?: (targetId: string) => void;
    public onRequestUseItem?: (itemId: string) => void;
    public onRequestDropItem?: (itemId: string, x: number, y: number) => void;
    public onRequestEquipItem?: (itemId: string) => void;
    public onRequestUnequipItem?: (slot: string) => void;
    public onRequestMoveItemInInventory?: (itemId: string, toIndex: number) => void;

    constructor() {
        super({ key: 'MainScene' });
    }

    public async loadMap() {
        // Obsoleto: O mapa agora é carregado via Chunks pelo SignalR
    }

    async create() {
        this.inputManager    = new InputManager(this);
        this.playerManager   = new PlayerManager(this);
        this.monsterManager  = new MonsterManager(this);
        this.itemManager     = new ItemManager(this);
        this.chunkManager    = new ChunkManager(this);
        this.inventoryManager = new InventoryManager();
        this.equipmentManager = new EquipmentManager();
        this.combatSystem    = new CombatSystem(this, this.playerManager, this.monsterManager);

        this.inventoryUI    = new InventoryUI();
        this.equipmentUI    = new EquipmentUI();
        this.dragDropHandler = new DragDropHandler(
            (itemId) => this.onRequestEquipItem?.(itemId),
            (slot)   => this.onRequestUnequipItem?.(slot),
            (itemId, toIndex) => this.onRequestMoveItemInInventory?.(itemId, toIndex),
        );
        this.dragDropHandler.setup(this.inventoryUI, this.equipmentUI);

        this.inventoryManager.onUseItem    = (itemId) => this.onRequestUseItem?.(itemId);
        this.inventoryManager.onDropItem   = (itemId, x, y) => this.onRequestDropItem?.(itemId, x, y);
        this.inventoryManager.onEquipItem  = (itemId) => this.onRequestEquipItem?.(itemId);

        this.inventoryUI.onSlotRightClick  = (itemId) => this.onRequestUseItem?.(itemId);
        this.equipmentUI.onSlotClick       = (slot)   => this.onRequestUnequipItem?.(slot);

        this.scene.launch('WorldMapScene');
        this.inputManager.setup();

        if (import.meta.env.DEV) {
            this.debugPanel = new DebugPanel();
        }

        this.monsterManager.onAttackMonster = (id) => {
            if (this.onAttackMonster) this.onAttackMonster(id);
        };
    }

    update() {
        if (!this.playerManager.myId || !this.onRequestMove) return;

        const myPlayer = this.playerManager.getMyPlayer();

        if (myPlayer) {
            this.chunkManager.update(myPlayer.gridPosition, 1);
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

        if (this.inputManager.isMapJustPressed()) {
            const mapScene = this.scene.get('WorldMapScene') as any;
            if (mapScene) mapScene.toggle();
        }

        if (this.inputManager.isInventoryJustPressed()) {
            console.log('Toggle Inventory UI');
            this.inventoryUI.toggle();
        }

        if (this.inputManager.isEquipmentJustPressed()) {
            console.log('Toggle Equipment UI');
            this.equipmentUI.toggle();
        }

        if (this.debugPanel) {
            this.debugPanel.update(this.monsterManager.getMonstersDict());
        }
    }

    // --- Inventory / Equipment events from SignalR ---

    public inventoryUpdated(items: ItemData[]): void {
        this.inventoryManager.onInventoryUpdated(items);
        this.inventoryUI.renderItems(items);
    }

    public equipmentUpdated(slots: Record<EquipmentSlot, ItemData | null>): void {
        this.equipmentManager.onEquipmentUpdated(slots);
        this.equipmentUI.renderSlots(slots);
        this.equipmentUI.updateStats(
            this.equipmentManager.getTotalAttackBonus(),
            this.equipmentManager.getTotalDefenseBonus(),
        );
    }

    // --- Chunk Logic ---
    public chunkLoaded(data: any) {
        this.chunkManager.handleChunkLoaded(data);
    }

    // --- Wrapper Methods ---
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
