// src/managers/PlayerManager.ts
import Phaser from 'phaser';
import { Player } from '../entities/Player';
import { GRID_SIZE, PLAYER_POSITION_OFFSET_X, PLAYER_POSITION_OFFSET_Y } from '../config/constants';
import type { PlayerPosData, Position } from '../types';

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
            player.updateHealthBar();
        }
    }

    private spawnNewPlayer(id: string, name: string, playerPosData: PlayerPosData, worldPosition: Position, isMe: boolean): void {
        const newPlayer = new Player(id, name, playerPosData, worldPosition, this.scene, GRID_SIZE);
        this.players[id] = newPlayer;

        if (isMe) {
            this.myId = id;
            this.scene.cameras.main.startFollow(newPlayer, true, 0.1, 0.1);
        }
    }

    private getWorldCoordinates(gridPosition: Position): Position {
        const px = (gridPosition.x * GRID_SIZE + (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_X;
        const py = (-gridPosition.y * GRID_SIZE - (GRID_SIZE / 2)) - PLAYER_POSITION_OFFSET_Y;
        return { x: px, y: py };
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