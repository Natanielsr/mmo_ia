// src/systems/CombatSystem.ts
import Phaser from 'phaser';
import { PlayerManager } from '../managers/PlayerManager';
import { MonsterManager } from '../managers/MonsterManager';

export class CombatSystem {
    private scene: Phaser.Scene;
    private playerManager: PlayerManager;
    private monsterManager: MonsterManager;

    constructor(scene: Phaser.Scene, playerManager: PlayerManager, monsterManager: MonsterManager) {
        this.scene = scene;
        this.playerManager = playerManager;
        this.monsterManager = monsterManager;
    }

    public attackNearestMonster(onAttackCallback?: (targetId: string) => void): void {
        const myPlayer = this.playerManager.getMyPlayer();
        if (!myPlayer || !onAttackCallback) return;

        let nearestMonster = null;
        const monsters = this.monsterManager.getMonstersDict();

        for (const id in monsters) {
            const monster = monsters[id];
            if (monster.isDead) continue;

            const dx = Math.round(Math.abs(myPlayer.gridPosition.x - monster.gridPosition.x));
            const dy = Math.round(Math.abs(myPlayer.gridPosition.y - monster.gridPosition.y));

            if (dx <= 1 && dy <= 1 && (dx + dy) > 0) {
                nearestMonster = monster;
                break;
            }
        }

        if (nearestMonster) {
            onAttackCallback(nearestMonster.id);
        }
    }

    public handlePlayerAttacked(data: { attackerId: string; targetId: string; damage: number }): void {
        const attacker = this.playerManager.getPlayer(data.attackerId);
        const targetPlayer = this.playerManager.getPlayer(data.targetId);
        const targetMonster = this.monsterManager.getMonster(data.targetId);

        if (attacker) {
            const targetX = targetPlayer?.x ?? targetMonster?.x ?? attacker.x;
            const targetY = targetPlayer?.y ?? targetMonster?.y ?? attacker.y;
            attacker.playAttackAnimation(targetX, targetY);
        }

        if (targetPlayer) {
            targetPlayer.takeDamage(data.damage);
            this.showDamageText(data.damage, targetPlayer.x, targetPlayer.y);
        }
    }

    public handleMonsterDamaged(data: { id: string; damage: number; currentHp: number }): void {
        const monster = this.monsterManager.getMonster(data.id);
        if (monster) {
            monster.takeDamage(data.damage);
            this.showDamageText(data.damage, monster.x, monster.y);
        }
    }

    private showDamageText(damage: number, x: number, y: number): void {
        console.log("Monster damaged " + damage);
        const damageText = this.scene.add.text(x, y - 50, `-${damage}`, {
            fontSize: '16px', color: '#FF5555', fontFamily: 'Inter',
            stroke: '#000000', strokeThickness: 3
        }).setOrigin(0.5).setDepth(10000);

        this.scene.tweens.add({
            targets: damageText,
            y: y - 100,
            alpha: 0,
            duration: 1000,
            ease: 'Power2',
            onComplete: () => damageText.destroy()
        });
    }
}