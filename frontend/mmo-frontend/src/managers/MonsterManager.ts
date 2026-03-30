// src/managers/MonsterManager.ts
import Phaser from 'phaser';
import { Monster } from '../entities/Monster';
import { GRID_SIZE } from '../config/constants';
import type { MonsterData } from '../types';

export class MonsterManager {
    private scene: Phaser.Scene;
    private monsters: Record<string, Monster> = {};
    public onAttackMonster?: (targetId: string) => void;

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public syncMonsters(monsterList: MonsterData[]): void {
        monsterList.forEach(data => this.syncMonster(data));
    }

    public syncMonster(monsterData: MonsterData): void {
        if (this.monsters[monsterData.id]) {
            this.monsters[monsterData.id].updateFromServer(monsterData);
        } else {
            this.spawnMonster(monsterData);
        }
    }

    private spawnMonster(monsterData: MonsterData): void {
        const newMonster = new Monster(monsterData, this.scene);
        this.monsters[monsterData.id] = newMonster;

        newMonster.setSize(GRID_SIZE, GRID_SIZE);
        newMonster.setInteractive();
        newMonster.on('pointerdown', () => {
            if (this.onAttackMonster) this.onAttackMonster(newMonster.id);
        });
    }

    public monsterDied(monsterId: string): void {
        const monster = this.monsters[monsterId];
        if (monster) {
            monster.die();
            this.scene.time.delayedCall(5000, () => {
                if (this.monsters[monsterId]) {
                    this.monsters[monsterId].destroy();
                    delete this.monsters[monsterId];
                }
            });
        }
    }

    public removeMonster(monsterId: string): void {
        const monster = this.monsters[monsterId];
        if (monster) {
            monster.destroy();
            delete this.monsters[monsterId];
        }
    }

    public getMonster(id: string): Monster | null {
        return this.monsters[id] || null;
    }

    public getMonstersDict(): Record<string, Monster> {
        return this.monsters;
    }
}