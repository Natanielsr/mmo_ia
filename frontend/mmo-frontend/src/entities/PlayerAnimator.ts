import Phaser from 'phaser';
import { getDirectionAnimation, getIdleFrame } from '../utils/playerAnimations';

export class PlayerAnimator {
    private scene: Phaser.Scene;
    private sprite: Phaser.GameObjects.Sprite;
    private container: Phaser.GameObjects.Container;
    private getIsDead: () => boolean;

    private weaponSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedWeaponTextureKey: string | null = null;
    private armorSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedArmorWalkKey: string | null = null;
    private equippedArmorSlashKey: string | null = null;
    private legsSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedLegsWalkKey: string | null = null;
    private equippedLegsSlashKey: string | null = null;
    private headSprite: Phaser.GameObjects.Sprite | null = null;
    private equippedHeadWalkKey: string | null = null;
    private equippedHeadSlashKey: string | null = null;

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
        if (this.armorSprite && this.equippedArmorWalkKey)
            this.armorSprite.play(`${this.equippedArmorWalkKey}-${this.facingDirection}`, true);
        if (this.legsSprite && this.equippedLegsWalkKey)
            this.legsSprite.play(`${this.equippedLegsWalkKey}-${this.facingDirection}`, true);
        if (this.headSprite && this.equippedHeadWalkKey)
            this.headSprite.play(`${this.equippedHeadWalkKey}-${this.facingDirection}`, true);
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
        if (this.armorSprite && this.equippedArmorWalkKey) {
            this.armorSprite.stop();
            this.armorSprite.setTexture(this.equippedArmorWalkKey);
            this.armorSprite.setFrame(idleFrame);
        }
        if (this.legsSprite && this.equippedLegsWalkKey) {
            this.legsSprite.stop();
            this.legsSprite.setTexture(this.equippedLegsWalkKey);
            this.legsSprite.setFrame(idleFrame);
        }
        if (this.headSprite && this.equippedHeadWalkKey) {
            this.headSprite.stop();
            this.headSprite.setTexture(this.equippedHeadWalkKey);
            this.headSprite.setFrame(idleFrame);
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
        if (this.armorSprite && this.equippedArmorSlashKey) {
            this.armorSprite.setTexture(this.equippedArmorSlashKey);
            this.armorSprite.play(`${this.equippedArmorSlashKey}-${animSuffix}`, true);
        }
        if (this.legsSprite && this.equippedLegsSlashKey) {
            this.legsSprite.setTexture(this.equippedLegsSlashKey);
            this.legsSprite.play(`${this.equippedLegsSlashKey}-${animSuffix}`, true);
        }
        if (this.headSprite && this.equippedHeadSlashKey) {
            this.headSprite.setTexture(this.equippedHeadSlashKey);
            this.headSprite.play(`${this.equippedHeadSlashKey}-${animSuffix}`, true);
        }

        this.sprite.once('animationcomplete', (animation: any) => {
            if (animation.key !== animKey) return;
            this.isAttacking = false;
            this.sprite.setTexture('hero', getIdleFrame(this.facingDirection));
            this.weaponSprite?.setVisible(false);
            this.weaponSprite?.stop();
            if (this.armorSprite && this.equippedArmorWalkKey) {
                this.armorSprite.setTexture(this.equippedArmorWalkKey);
                this.armorSprite.setFrame(getIdleFrame(this.facingDirection));
            }
            if (this.legsSprite && this.equippedLegsWalkKey) {
                this.legsSprite.setTexture(this.equippedLegsWalkKey);
                this.legsSprite.setFrame(getIdleFrame(this.facingDirection));
            }
            if (this.headSprite && this.equippedHeadWalkKey) {
                this.headSprite.setTexture(this.equippedHeadWalkKey);
                this.headSprite.setFrame(getIdleFrame(this.facingDirection));
            }
            this.scheduleStopWalking();
        });
    }

    setEquippedArmorOverlay(walkKey: string | null, slashKey: string | null): void {
        if (!walkKey) {
            this.armorSprite?.destroy();
            this.armorSprite = null;
            this.equippedArmorWalkKey = null;
            this.equippedArmorSlashKey = null;
            return;
        }
        this.equippedArmorWalkKey = walkKey;
        this.equippedArmorSlashKey = slashKey;
        if (!this.armorSprite) {
            this.armorSprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            this.armorSprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(this.armorSprite);
        }
    }

    setEquippedLegsOverlay(walkKey: string | null, slashKey: string | null): void {
        if (!walkKey) {
            this.legsSprite?.destroy();
            this.legsSprite = null;
            this.equippedLegsWalkKey = null;
            this.equippedLegsSlashKey = null;
            return;
        }
        this.equippedLegsWalkKey = walkKey;
        this.equippedLegsSlashKey = slashKey;
        if (!this.legsSprite) {
            this.legsSprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            this.legsSprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(this.legsSprite);
        }
    }

    setEquippedHeadOverlay(walkKey: string | null, slashKey: string | null): void {
        if (!walkKey) {
            this.headSprite?.destroy();
            this.headSprite = null;
            this.equippedHeadWalkKey = null;
            this.equippedHeadSlashKey = null;
            return;
        }
        this.equippedHeadWalkKey = walkKey;
        this.equippedHeadSlashKey = slashKey;
        if (!this.headSprite) {
            this.headSprite = this.scene.add.sprite(0, 0, walkKey, getIdleFrame(this.facingDirection));
            this.headSprite.setDisplaySize(this.sprite.displayWidth, this.sprite.displayHeight);
            this.container.add(this.headSprite);
        }
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
        this.armorSprite?.destroy();
        this.legsSprite?.destroy();
        this.headSprite?.destroy();
    }
}
