import Phaser from 'phaser';
import { getDirectionAnimation, getIdleFrame } from '../utils/playerAnimations';

export class PlayerAnimator {
    private scene: Phaser.Scene;
    private sprite: Phaser.GameObjects.Sprite;
    private container: Phaser.GameObjects.Container;
    private getIsDead: () => boolean;

    private weaponSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedWeaponWalkKey: string | null = null;
    private equippedWeaponSlashKey: string | null = null;
    private overlaySprites   = new Map<string, Phaser.GameObjects.Sprite>();
    private overlayWalkKeys  = new Map<string, string>();
    private overlaySlashKeys = new Map<string, string | null>();
    private overlayHurtKeys  = new Map<string, string | null>();
    private equippedWeaponHurtKey: string | null = null;

    private stopWalkingTimer?: Phaser.Time.TimerEvent;
    public isAttacking: boolean = false;
    public facingDirection: string = 'south';

    constructor(
        scene: Phaser.Scene,
        sprite: Phaser.GameObjects.Sprite,
        container: Phaser.GameObjects.Container,
        getIsDead: () => boolean
    ) {
        this.scene = scene;
        this.sprite = sprite;
        this.container = container;
        this.getIsDead = getIsDead;
    }

    setInitialPose(): void {
        this.sprite.setFrame(18);
    }

    playWalkAnimation(dx: number, dy: number): void {
        const animKey = getDirectionAnimation(dx, dy);
        if (!animKey) return;
        this.facingDirection = animKey.replace('walk-', '');
        if (this.isAttacking || this.getIsDead()) return;
        if (this.sprite.texture.key !== 'hero') this.sprite.setTexture('hero');
        this.sprite.play(animKey, true);
        if (this.weaponSprite && this.equippedWeaponWalkKey) {
            this.weaponSprite.setVisible(true);
            this.weaponSprite.play(`${this.equippedWeaponWalkKey}-${this.facingDirection}`, true);
        }
        for (const [slot, sprite] of this.overlaySprites) {
            const walkKey = this.overlayWalkKeys.get(slot);
            if (walkKey) sprite.play(`${walkKey}-${this.facingDirection}`, true);
        }
    }

    cancelStopWalkingTimer(): void {
        if (this.stopWalkingTimer) {
            this.stopWalkingTimer.destroy();
            this.stopWalkingTimer = undefined;
        }
    }

    scheduleStopWalking(): void {
        this.stopWalkingTimer = this.scene.time.delayedCall(100, () => this.stopWalking());
    }

    private stopWalking(): void {
        if (this.isAttacking || this.getIsDead()) return;
        this.sprite.stop();
        const idleFrame = getIdleFrame(this.facingDirection);
        this.sprite.setFrame(idleFrame);
        if (this.weaponSprite && this.equippedWeaponWalkKey) {
            this.weaponSprite.stop();
            this.weaponSprite.setTexture(this.equippedWeaponWalkKey, idleFrame);
        }
        for (const [slot, sprite] of this.overlaySprites) {
            const walkKey = this.overlayWalkKeys.get(slot);
            if (walkKey) {
                sprite.stop();
                sprite.setTexture(walkKey);
                sprite.setFrame(idleFrame);
            }
        }
    }

    playAttackAnimation(playerX: number, playerY: number, targetX: number, targetY: number): void {
        const dx = targetX - playerX;
        const dy = targetY - playerY;

        let animSuffix = this.facingDirection;
        if (Math.abs(dx) > 10 || Math.abs(dy) > 10) {
            animSuffix = Math.abs(dx) > Math.abs(dy)
                ? (dx > 0 ? 'east' : 'west')
                : (dy > 0 ? 'south' : 'north');
            this.facingDirection = animSuffix;
        }

        const animKey = `attack-${animSuffix}`;
        this.isAttacking = true;

        this.sprite.setTexture('hero_slash');
        this.sprite.play(animKey, true);

        if (this.weaponSprite && this.equippedWeaponSlashKey) {
            this.weaponSprite.setVisible(true);
            this.weaponSprite.play(`${this.equippedWeaponSlashKey}-${animSuffix}`, true);
        }
        for (const [slot, sprite] of this.overlaySprites) {
            const slashKey = this.overlaySlashKeys.get(slot);
            if (slashKey) {
                sprite.setTexture(slashKey);
                sprite.play(`${slashKey}-${animSuffix}`, true);
            }
        }

        this.sprite.once('animationcomplete', (animation: any) => {
            if (animation.key !== animKey) return;
            this.isAttacking = false;
            this.sprite.setTexture('hero', getIdleFrame(this.facingDirection));
            if (this.weaponSprite && this.equippedWeaponWalkKey) {
                this.weaponSprite.setTexture(this.equippedWeaponWalkKey, getIdleFrame(this.facingDirection));
            }
            for (const [slot, sprite] of this.overlaySprites) {
                const walkKey = this.overlayWalkKeys.get(slot);
                if (walkKey) {
                    sprite.setTexture(walkKey);
                    sprite.setFrame(getIdleFrame(this.facingDirection));
                }
            }
            this.scheduleStopWalking();
        });
    }

    setEquippedBodyOverlay(slot: string, walkKey: string | null, slashKey: string | null, hurtKey: string | null = null): void {
        if (!walkKey) {
            this.overlaySprites.get(slot)?.destroy();
            this.overlaySprites.delete(slot);
            this.overlayWalkKeys.delete(slot);
            this.overlaySlashKeys.delete(slot);
            this.overlayHurtKeys.delete(slot);
            return;
        }
        this.overlayWalkKeys.set(slot, walkKey);
        this.overlaySlashKeys.set(slot, slashKey);
        this.overlayHurtKeys.set(slot, hurtKey);
        if (!this.overlaySprites.has(slot)) {
            const sprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            sprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(sprite);
            this.overlaySprites.set(slot, sprite);
        } else {
            this.overlaySprites.get(slot)!.setTexture(walkKey, getIdleFrame(this.facingDirection));
        }
        if (this.weaponSprite) this.container.bringToTop(this.weaponSprite);
        const shieldSprite = this.overlaySprites.get('Shield');
        if (shieldSprite) this.container.bringToTop(shieldSprite);
    }

    setEquippedWeaponOverlay(walkKey: string | null, slashKey: string | null, hurtKey: string | null = null): void {
        if (!walkKey) {
            this.weaponSprite?.destroy();
            this.weaponSprite = null;
            this.equippedWeaponWalkKey = null;
            this.equippedWeaponSlashKey = null;
            this.equippedWeaponHurtKey = null;
            return;
        }
        this.equippedWeaponWalkKey = walkKey;
        this.equippedWeaponSlashKey = slashKey;
        this.equippedWeaponHurtKey = hurtKey;
        if (!this.weaponSprite) {
            this.weaponSprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            this.weaponSprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(this.weaponSprite);
        } else {
            this.weaponSprite.setTexture(walkKey, getIdleFrame(this.facingDirection));
        }
        this.container.bringToTop(this.weaponSprite);
        const shieldSprite = this.overlaySprites.get('Shield');
        if (shieldSprite) this.container.bringToTop(shieldSprite);
    }

    playTakeDamageFlash(): void {
        const allSprites = [this.sprite, ...this.overlaySprites.values(), this.weaponSprite].filter(Boolean) as Phaser.GameObjects.Sprite[];
        this.scene.tweens.add({
            targets: allSprites,
            tint: 0xff0000,
            duration: 100,
            yoyo: true,
            onComplete: () => {
                if (!this.getIsDead()) allSprites.forEach(s => s.clearTint());
            }
        });
    }

    playDieEffect(): void {
        this.cancelStopWalkingTimer();
        this.isAttacking = false;
        this.sprite.setTexture('hero_hurt');
        this.sprite.play('hero_hurt', true);
        this.sprite.once('animationcomplete', () => {
            this.sprite.setTint(0x666666);
        });

        for (const [slot, sprite] of this.overlaySprites) {
            const hurtKey = this.overlayHurtKeys.get(slot) ?? null;
            if (hurtKey) {
                sprite.setTexture(hurtKey);
                sprite.play(hurtKey, true);
                sprite.once('animationcomplete', () => sprite.setTint(0x666666));
            } else {
                sprite.setVisible(false);
            }
        }

        if (this.weaponSprite) {
            if (this.equippedWeaponHurtKey) {
                this.weaponSprite.setTexture(this.equippedWeaponHurtKey);
                this.weaponSprite.play(this.equippedWeaponHurtKey, true);
                this.weaponSprite.once('animationcomplete', () => this.weaponSprite!.setTint(0x666666));
            } else {
                this.weaponSprite.setVisible(false);
            }
        }
    }

    playCrunchEffect(x: number, y: number, nameTextOffsetY: number): void {
        const text = this.scene.add.text(x, y - nameTextOffsetY - 25, 'Crunch!', {
            fontSize: '18px', color: '#FFA500', fontFamily: 'Inter',
            stroke: '#7A3E00', strokeThickness: 3, fontStyle: 'bold'
        }).setOrigin(0.5).setDepth(10001);
        this.scene.tweens.add({
            targets: text,
            y: text.y - 50,
            alpha: 0,
            duration: 1000,
            ease: 'Cubic.easeOut',
            onComplete: () => text.destroy(),
        });
    }

    playHealEffect(x: number, y: number, nameTextOffsetY: number): void {
        const healText = this.scene.add.text(x, y - nameTextOffsetY - 25, '+HP', {
            fontSize: '18px', color: '#00ff88', fontFamily: 'Inter', stroke: '#004422', strokeThickness: 3, fontStyle: 'bold'
        }).setOrigin(0.5).setDepth(10001);

        this.sprite.setTint(0x00ff88);
        this.scene.time.delayedCall(200, () => {
            if (!this.getIsDead()) this.sprite.clearTint();
        });

        this.scene.tweens.add({
            targets: healText,
            y: healText.y - 50,
            alpha: 0,
            duration: 800,
            ease: 'Cubic.easeOut',
            onComplete: () => healText.destroy(),
        });
    }

    playLevelUpEffect(x: number, y: number, nameTextOffsetY: number): void {
        this.scene.tweens.add({
            targets: this.sprite,
            scaleY: this.sprite.scaleY * 1.3,
            scaleX: this.sprite.scaleX * 1.1,
            duration: 200,
            yoyo: true,
            ease: 'Quad.easeInOut'
        });

        this.scene.tweens.add({
            targets: this.sprite,
            tint: 0xffff00,
            duration: 300,
            yoyo: true,
            onComplete: () => {
                if (!this.getIsDead()) this.sprite.clearTint();
            }
        });

        const levelUpText = this.scene.add.text(x, y - nameTextOffsetY - 25, 'LEVEL UP!', {
            fontSize: '20px', color: '#ffb700', fontFamily: 'Inter', stroke: '#000', strokeThickness: 4, fontStyle: 'bold'
        }).setOrigin(0.5).setDepth(10001);

        this.scene.tweens.add({
            targets: levelUpText,
            y: levelUpText.y - 50,
            alpha: 0,
            duration: 2000,
            ease: 'Cubic.easeOut',
            onComplete: () => levelUpText.destroy()
        });

        const particles = this.scene.add.particles(x, y - 10, 'hero', {
            speed: { min: -100, max: 100 },
            angle: { min: 0, max: 360 },
            scale: { start: 0.5, end: 0 },
            blendMode: 'ADD',
            lifespan: 1000,
            gravityY: 100,
            emitting: false
        });
        particles.setDepth(10000);
        particles.explode(20);
        this.scene.time.delayedCall(1000, () => particles.destroy());
    }

    destroy(): void {
        this.weaponSprite?.destroy();
        for (const sprite of this.overlaySprites.values()) sprite.destroy();
        this.overlaySprites.clear();
    }
}
