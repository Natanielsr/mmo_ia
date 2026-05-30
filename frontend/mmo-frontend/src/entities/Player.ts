import Phaser from 'phaser';
import type { Position, PlayerData } from '../types';
import { getPlayerHealthColor } from '../utils/healthColors';
import { PlayerAnimator } from './PlayerAnimator';

export class Player extends Phaser.GameObjects.Container {
    public id: string;
    public name: string;
    private sprite: Phaser.GameObjects.Sprite;
    private nameText: Phaser.GameObjects.Text;
    private nameTextOffsetY: number;
    public gridPosition: Position;
    public worldPosition: Position;
    public hp: number;
    public maxHp: number;
    public level: number;
    public experience: number;
    public isDead: boolean = false;
    private healthBar: Phaser.GameObjects.Graphics;
    private healthBarBackground: Phaser.GameObjects.Graphics;
    private animator: PlayerAnimator;

    public get isAttacking(): boolean { return this.animator.isAttacking; }

    constructor(
        id: string,
        name: string,
        playerPosData: PlayerData,
        worldPosition: Position,
        scene: Phaser.Scene,
        gridSize: number) {
        super(scene, worldPosition.x, worldPosition.y);
        this.id = id;
        this.worldPosition = worldPosition;
        const pos = (playerPosData.position ?? (playerPosData as any).Position) as any;
        this.gridPosition = { x: pos.x ?? pos.X, y: pos.y ?? pos.Y };
        this.name = name;
        const data = playerPosData as any;
        this.hp = Number(data.hp ?? data.Hp ?? 100);
        this.maxHp = Number(data.maxHp ?? data.MaxHp ?? 100);
        this.level = Number(data.level ?? data.Level ?? 1);
        this.experience = Number(data.experience ?? data.Experience ?? 0);
        this.isDead = Boolean(data.isDead ?? data.IsDead ?? false);

        this.sprite = scene.add.sprite(0, 0, 'hero', 0);
        this.sprite.setDisplaySize(gridSize * 1.25, gridSize * 1.25);
        this.sprite.setDepth(499);

        this.nameTextOffsetY = (gridSize / 2) + 15;
        this.nameText = scene.add.text(worldPosition.x, worldPosition.y - this.nameTextOffsetY, `[Lv ${this.level}] ${this.name}`, {
            fontSize: '14px', color: '#fff', fontFamily: 'Inter', stroke: '#000', strokeThickness: 2
        }).setOrigin(0.5);
        this.nameText.setDepth(10000);

        this.healthBarBackground = scene.add.graphics();
        this.healthBarBackground.fillStyle(0x000000, 0.5);
        this.healthBarBackground.fillRect(-25, -gridSize / 2 - 5, 50, 6);

        this.healthBar = scene.add.graphics();
        this.updateHealthBar();

        this.add([this.sprite, this.healthBarBackground, this.healthBar]);
        scene.add.existing(this);
        this.setDepth(worldPosition.y);

        this.animator = new PlayerAnimator(scene, this.sprite, this, () => this.isDead);
        this.animator.setInitialPose();
    }

    public updateHealthBar(): void {
        this.healthBar.clear();
        const healthPercent = Math.max(0, this.hp / this.maxHp);
        const color = getPlayerHealthColor(healthPercent);
        this.healthBar.fillStyle(color, 1);
        this.healthBar.fillRect(-25, -37, 50 * healthPercent, 6);
    }

    public takeDamage(damage: number): void {
        this.hp = Math.max(0, this.hp - damage);
        this.updateHealthBar();
        this.animator.playTakeDamageFlash();
    }

    public die(): void {
        if (this.isDead) return;
        this.isDead = true;
        this.animator.playDieEffect();
        this.nameText.setText(`[Lv ${this.level}] ${this.name} 💀`);
        this.updateHealthBar();
    }

    public move(worldPos: Position, duration: number): void {
        const dx = worldPos.x - this.x;
        const dy = worldPos.y - this.y;

        this.animator.cancelStopWalkingTimer();
        this.animator.playWalkAnimation(dx, dy);

        this.scene.tweens.killTweensOf(this);
        this.scene.tweens.killTweensOf(this.nameText);

        this.scene.tweens.add({
            targets: this,
            x: worldPos.x,
            y: worldPos.y,
            duration: duration,
            ease: 'Linear',
            onUpdate: () => {
                this.setDepth(this.y);
            },
            onComplete: () => {
                this.animator.scheduleStopWalking();
            }
        });

        this.scene.tweens.add({
            targets: this.nameText,
            x: worldPos.x,
            y: worldPos.y - this.nameTextOffsetY,
            duration: duration,
            ease: 'Linear'
        });
    }

    public setEquippedBodyOverlay(slot: string, walkKey: string | null, slashKey: string | null, hurtKey: string | null = null): void {
        this.animator.setEquippedBodyOverlay(slot, walkKey, slashKey, hurtKey);
    }

    public setEquippedWeaponOverlay(walkKey: string | null, slashKey: string | null, hurtKey: string | null = null): void {
        this.animator.setEquippedWeaponOverlay(walkKey, slashKey, hurtKey);
    }

    public playAttackAnimation(targetX: number, targetY: number): void {
        this.animator.playAttackAnimation(this.x, this.y, targetX, targetY);
    }

    public playHealEffect(): void {
        this.animator.playHealEffect(this.x, this.y, this.nameTextOffsetY);
    }

    public playCrunchEffect(): void {
        this.animator.playCrunchEffect(this.x, this.y, this.nameTextOffsetY);
    }

    public updateLevelAndXP(newLevel: number, newExperience: number): void {
        if (newLevel > this.level) {
            this.animator.playLevelUpEffect(this.x, this.y, this.nameTextOffsetY);
        }
        this.level = newLevel;
        this.experience = newExperience;
        this.nameText.setText(this.isDead
            ? `[Lv ${this.level}] ${this.name} 💀`
            : `[Lv ${this.level}] ${this.name}`
        );
    }

    public destroy(fromScene?: boolean): void {
        this.sprite.destroy();
        this.nameText.destroy();
        this.animator.destroy();
        super.destroy(fromScene);
    }
}
