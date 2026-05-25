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
import { CombatSystem } from '../systems/CombatSystem';
import type { MonsterData, ItemData, EquipmentSlot } from '../types';
import type { Player } from '../entities/Player';
import type { Monster } from '../entities/Monster';
import { useGameStore } from '../stores/gameStore';
import { isMobileDevice } from '../utils/device';
import { getWeaponOverlayConfig } from '../config/overlays/weaponOverlayRegistry';
import { getArmorOverlayConfig } from '../config/overlays/armorOverlayRegistry';
import { getLegsOverlayConfig } from '../config/overlays/legsOverlayRegistry';
import { getHeadOverlayConfig } from '../config/overlays/headOverlayRegistry';
import { DevConsole } from './DevConsole';


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
    private devConsole?: DevConsole;

    public onRequestMove?: (direction: string) => void;
    public onAttackMonster?: (targetId: string) => void;
    public onRequestUseItem?: (itemId: string) => void;
    public onRequestDropItem?: (itemId: string, x: number, y: number) => void;
    public onRequestEquipItem?: (itemId: string) => void;
    public onRequestUnequipItem?: (slot: string) => void;
    public onRequestMoveItemInInventory?: (itemId: string, toIndex: number) => void;
    private signalRService?: any;

    constructor() {
        super({ key: 'MainScene' });
    }

    public setSignalRService(service: any): void {
        this.signalRService = service;
    }

    public async loadMap() {
        // Obsoleto: O mapa agora é carregado via Chunks pelo SignalR
    }

    public openDefaultPanels(): void {
        const store = useGameStore();
        store.panelBarVisible = true;
        if (!isMobileDevice()) {
            store.openGearPanel();
        }
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

        this.inventoryManager.onUseItem    = (itemId: string) => {
            this.playerManager.getMyPlayer()?.playHealEffect();
            this.onRequestUseItem?.(itemId);
        };
        this.inventoryManager.onDropItem   = (itemId: string, x: number, y: number) => this.onRequestDropItem?.(itemId, x, y);
        this.inventoryManager.onEquipItem  = (itemId: string) => this.onRequestEquipItem?.(itemId);

        useGameStore().registerCallbacks({
            onEquip:           (itemId: string) => this.onRequestEquipItem?.(itemId),
            onUnequip:         (slot: string)   => this.onRequestUnequipItem?.(slot),
            onMoveInInventory: (itemId: string, toIndex: number) => this.onRequestMoveItemInInventory?.(itemId, toIndex),
            onUseItem:         (itemId: string) => {
                this.playerManager.getMyPlayer()?.playHealEffect();
                this.onRequestUseItem?.(itemId);
            },
            onDropItem:        (itemId: string, x: number, y: number) => this.onRequestDropItem?.(itemId, x, y),
        });

        this.scene.launch('WorldMapScene');
        this.inputManager.setup();

        if (import.meta.env.DEV) {
            this.debugPanel = new DebugPanel();
            if (this.signalRService) {
                this.devConsole = new DevConsole(this.signalRService);
            }
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
            useGameStore().setPosition(myPlayer.gridPosition.x, myPlayer.gridPosition.y);

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

        if (this.inputManager.isConsoleJustPressed()) {
            this.devConsole?.toggle();
        }

        if (this.inputManager.isMapJustPressed()) {
            const mapScene = this.scene.get('WorldMapScene') as any;
            if (mapScene) mapScene.toggle();
        }

        if (this.inputManager.isInventoryJustPressed() || this.inputManager.isEquipmentJustPressed()) {
            useGameStore().toggleGearPanel();
        }

        if (this.inputManager.isAttributesJustPressed()) {
            useGameStore().toggleAttributesModal();
        }

        if (this.debugPanel) {
            this.debugPanel.update(this.monsterManager.getMonstersDict());
        }
    }

    // --- Inventory / Equipment events from SignalR ---

    public inventoryUpdated(items: ItemData[]): void {
        this.inventoryManager.onInventoryUpdated(items);
        useGameStore().updateInventory(items);
    }

    public equipmentUpdated(slots: Record<EquipmentSlot, ItemData | null>): void {
        this.equipmentManager.onEquipmentUpdated(slots);
        const store = useGameStore();
        store.updateEquipment(slots);
        store.setEquipStats(
            this.equipmentManager.getTotalAttackBonus(),
            this.equipmentManager.getTotalDefenseBonus(),
        );

        const weaponItem = slots['Weapon'];
        const weaponCfg = weaponItem ? getWeaponOverlayConfig(weaponItem.name) : null;
        this.playerManager.getMyPlayer()?.setEquippedWeaponOverlay(weaponCfg?.textureKey ?? null);

        const armorItem = slots['Chest'];
        const armorCfg = armorItem ? getArmorOverlayConfig(armorItem.name) : null;
        this.playerManager.getMyPlayer()?.setEquippedArmorOverlay(armorCfg?.walkTextureKey ?? null, armorCfg?.slashTextureKey ?? null);

        const legsItem = slots['Legs'];
        const legsCfg = legsItem ? getLegsOverlayConfig(legsItem.name) : null;
        this.playerManager.getMyPlayer()?.setEquippedLegsOverlay(legsCfg?.walkTextureKey ?? null, legsCfg?.slashTextureKey ?? null);

        const helmetItem = slots['Helmet'];
        const helmetCfg = helmetItem ? getHeadOverlayConfig(helmetItem.name) : null;
        this.playerManager.getMyPlayer()?.setEquippedHeadOverlay(helmetCfg?.walkTextureKey ?? null, helmetCfg?.slashTextureKey ?? null);
    }

    public playerWeaponEquipped(playerId: string, weaponName: string | null): void {
        const player = this.playerManager.getPlayer(playerId);
        if (!player) return;
        const cfg = weaponName ? getWeaponOverlayConfig(weaponName) : null;
        player.setEquippedWeaponOverlay(cfg?.textureKey ?? null);
    }

    public playerArmorEquipped(playerId: string, armorName: string | null): void {
        const player = this.playerManager.getPlayer(playerId);
        if (!player) return;
        const cfg = armorName ? getArmorOverlayConfig(armorName) : null;
        player.setEquippedArmorOverlay(cfg?.walkTextureKey ?? null, cfg?.slashTextureKey ?? null);
    }

    public playerLegsEquipped(playerId: string, legsName: string | null): void {
        const player = this.playerManager.getPlayer(playerId);
        if (!player) return;
        const cfg = legsName ? getLegsOverlayConfig(legsName) : null;
        player.setEquippedLegsOverlay(cfg?.walkTextureKey ?? null, cfg?.slashTextureKey ?? null);
    }

    public playerHelmetEquipped(playerId: string, helmetName: string | null): void {
        const player = this.playerManager.getPlayer(playerId);
        if (!player) return;
        const cfg = helmetName ? getHeadOverlayConfig(helmetName) : null;
        player.setEquippedHeadOverlay(cfg?.walkTextureKey ?? null, cfg?.slashTextureKey ?? null);
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
