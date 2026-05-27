// src/managers/PlayerManager.ts
import Phaser from 'phaser';
import { Player } from '../entities/Player';
import { GRID_SIZE, PLAYER_POSITION_OFFSET_X, PLAYER_POSITION_OFFSET_Y } from '../config/constants';
import { gridToWorld } from '../utils/coords';
import type { PlayerData, Position } from '../types';
import { getWeaponOverlayConfig, getBodyOverlayConfig } from '../config/overlays';

export class PlayerManager {
    private scene: Phaser.Scene;
    private players: Record<string, Player> = {};
    public myId: string | null = null;
    private minTimeBetweenMovesMs: number = 1000 / 2.0; // Baseado no playerSpeed 2.0

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public updatePlayerPosition(playerData: any, isMe: boolean = false): void {
        const id = playerData.id ?? playerData.Id;
        const pos = playerData.position ?? playerData.Position;
        if (!pos) return;

        const gridPosition = { x: pos.x ?? pos.X, y: pos.y ?? pos.Y };
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
        const player = this.players[id];
        if (player) {
            player.hp = statusData.hp ?? statusData.Hp ?? player.hp;
            player.maxHp = statusData.maxHp ?? statusData.MaxHp ?? player.maxHp;
            player.isDead = statusData.isDead ?? statusData.IsDead ?? player.isDead;
            
            const newLevel = statusData.level ?? statusData.Level ?? player.level;
            const newXP = statusData.experience ?? statusData.Experience ?? player.experience;
            player.updateLevelAndXP(newLevel, newXP);
            
            player.updateHealthBar();
        }
    }

    private spawnNewPlayer(id: string, name: string, playerPosData: PlayerData, worldPosition: Position, isMe: boolean): void {
        const newPlayer = new Player(id, name, playerPosData, worldPosition, this.scene, GRID_SIZE);
        this.players[id] = newPlayer;

        const equippedItems: Record<string, string | null> = (playerPosData as any).equippedItems ?? (playerPosData as any).EquippedItems ?? {};
        for (const [slot, itemName] of Object.entries(equippedItems)) {
            if (!itemName) continue;
            if (slot === 'Weapon') {
                const cfg = getWeaponOverlayConfig(itemName);
                newPlayer.setEquippedWeaponOverlay(cfg?.walkTextureKey ?? null, cfg?.slashTextureKey ?? null);
            } else {
                const cfg = getBodyOverlayConfig(slot, itemName);
                newPlayer.setEquippedBodyOverlay(slot, cfg?.walkTextureKey ?? null, cfg?.slashTextureKey ?? null);
            }
        }

        if (isMe) {
            this.myId = id;
            this.scene.cameras.main.startFollow(newPlayer, true, 1, 1);
        }
    }

    private getWorldCoordinates(gridPosition: Position): Position {
        return gridToWorld(gridPosition, PLAYER_POSITION_OFFSET_X, PLAYER_POSITION_OFFSET_Y);
    }

    public getMyPlayer(): Player | null {
        return this.myId ? this.players[this.myId] || null : null;
    }

    public getPlayer(id: string): Player | null {
        return this.players[String(id)] || null;
    }

    public removePlayer(id: string): void {
        if (this.players[id]) {
            this.players[id].destroy();
            delete this.players[id];
        }
    }

    public getPlayersDict(): Record<string, Player> {
        return this.players;
    }
}