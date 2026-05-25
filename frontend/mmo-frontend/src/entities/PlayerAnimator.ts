import Phaser from 'phaser';
import { getDirectionAnimation, getIdleFrame } from '../utils/playerAnimations';

export class PlayerAnimator {
    private scene: Phaser.Scene;
    private sprite: Phaser.GameObjects.Sprite;
    private container: Phaser.GameObjects.Container;
    private getIsDead: () => boolean;

    private weaponSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedWeaponTextureKey: string | null = null;
    private overlaySprites   = new Map<string, Phaser.GameObjects.Sprite>();
    private overlayWalkKeys  = new Map<string, string>();
    private overlaySlashKeys = new Map<string, string | null>();

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
        if (this.isAttacking) return;
        if (this.sprite.texture.key !== 'hero') this.sprite.setTexture('hero');
        this.sprite.play(animKey, true);
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
        if (this.isAttacking) return;
        this.sprite.stop();
        const idleFrame = getIdleFrame(this.facingDirection);
        this.sprite.setFrame(idleFrame);
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

        if (this.weaponSprite && this.equippedWeaponTextureKey) {
            this.weaponSprite.setVisible(true);
            this.weaponSprite.play(`${this.equippedWeaponTextureKey}-${animSuffix}`, true);
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
            this.weaponSprite?.setVisible(false);
            this.weaponSprite?.stop();
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

    setEquippedBodyOverlay(slot: string, walkKey: string | null, slashKey: string | null): void {
        if (!walkKey) {
            this.overlaySprites.get(slot)?.destroy();
            this.overlaySprites.delete(slot);
            this.overlayWalkKeys.delete(slot);
            this.overlaySlashKeys.delete(slot);
            return;
        }
        this.overlayWalkKeys.set(slot, walkKey);
        this.overlaySlashKeys.set(slot, slashKey);
        if (!this.overlaySprites.has(slot)) {
            const sprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            sprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(sprite);
            this.overlaySprites.set(slot, sprite);
        }
        const shieldSprite = this.overlaySprites.get('Shield');
        if (shieldSprite) this.container.bringToTop(shieldSprite);
    }

    setEquippedWeaponOverlay(textureKey: string | null): void {
        if (!textureKey) {
            this.weaponSprite?.destroy();
            this.weaponSprite = null;
            this.equippedWeaponTextureKey = null;
            return;
        }
        this.equippedWeaponTextureKey = textureKey;
        if (!this.weaponSprite) {
            this.weaponSprite = this.scene.add.sprite(0, 0, textureKey, 0);
            this.weaponSprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.weaponSprite.setVisible(false);
            this.container.add(this.weaponSprite);
        }
    }

    playTakeDamageFlash(): void {
        this.scene.tweens.add({
            targets: this.sprite,
            tint: 0xff0000,
            duration: 100,
            yoyo: true,
            onComplete: () => {
                if (!this.getIsDead()) this.sprite.clearTint();
            }
        });
    }

    playDieEffect(): void {
        this.sprite.setAngle(90);
        this.sprite.setTint(0x666666);
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
