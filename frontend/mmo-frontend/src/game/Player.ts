import Phaser from 'phaser';
import type { Position, PlayerPosData } from '../types';

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
    public isDead: boolean = false;
    private healthBar: Phaser.GameObjects.Graphics;
    private healthBarBackground: Phaser.GameObjects.Graphics;
    private stopWalkingTimer?: Phaser.Time.TimerEvent;
    public isAttacking: boolean = false;
    private facingDirection: string = 'south';

    constructor(
        id: string,
        name: string,
        playerPosData: PlayerPosData, // Modified to receive entire playerData
        worldPosition: Position,
        scene: Phaser.Scene,
        gridSize: number) {
        super(scene, worldPosition.x, worldPosition.y);
        this.id = id;
        this.worldPosition = worldPosition
        this.gridPosition = playerPosData.position;
        this.name = name;
        // Inicializa com valores seguros, buscando do playerData se ainda estiverem lá (casing flexível)
        const data = playerPosData as any;
        this.hp = Number(data.hp ?? data.Hp ?? 100);
        this.maxHp = Number(data.maxHp ?? data.MaxHp ?? 100);
        this.isDead = Boolean(data.isDead ?? data.IsDead ?? false);

        // 1. O Sprite fica na coordenada (0,0) local do Container
        this.sprite = scene.add.sprite(0, 0, 'hero', 0);
        this.sprite.setDisplaySize(gridSize * 1.25, gridSize * 1.25);
        this.sprite.setDepth(499); // Abaixo do texto

        // 2. Texto do nome fica FORA do container para controlar depth global da cena
        this.nameTextOffsetY = (gridSize / 2) + 15;
        this.nameText = scene.add.text(worldPosition.x, worldPosition.y - this.nameTextOffsetY, name, {
            fontSize: '14px', color: '#fff', fontFamily: 'Inter', stroke: '#000', strokeThickness: 2
        }).setOrigin(0.5);
        this.nameText.setDepth(10000); // acima de tudo

        // 3. Health bar (Background)
        this.healthBarBackground = scene.add.graphics();
        this.healthBarBackground.fillStyle(0x000000, 0.5);
        this.healthBarBackground.fillRect(-25, -gridSize / 2 - 5, 50, 6);

        // 4. Health bar (Foreground)
        this.healthBar = scene.add.graphics();
        this.updateHealthBar();

        // 5. Adiciona sprite e barras como filhos do Container
        this.add([this.sprite, this.healthBarBackground, this.healthBar]);

        // 6. Registra este Container na Cena para ser renderizado
        scene.add.existing(this);

        // 7. Container do player acima dos objetos do mapa
        this.setDepth(worldPosition.y);

        // Define a pose inicial
        this.setInitialPose();
    }

    public updateHealthBar() {
        this.healthBar.clear();
        const healthPercent = Math.max(0, this.hp / this.maxHp);
        const color = healthPercent > 0.5 ? 0x00ff00 : (healthPercent > 0.25 ? 0xffff00 : 0xff0000);

        this.healthBar.fillStyle(color, 1);
        this.healthBar.fillRect(-25, -37, 50 * healthPercent, 6);
    }

    public takeDamage(damage: number): void {
        this.hp = Math.max(0, this.hp - damage);
        this.updateHealthBar();

        // Flash vermelho no sprite
        this.scene.tweens.add({
            targets: this.sprite,
            tint: 0xff0000,
            duration: 100,
            yoyo: true,
            onComplete: () => {
                if (!this.isDead) this.sprite.clearTint();
            }
        });
    }

    public die(): void {
        if (this.isDead) return;
        this.isDead = true;
        this.sprite.setAngle(90); // Deita o personagem
        this.sprite.setTint(0x666666);
        this.nameText.setText(`${this.name} 💀`);
        this.updateHealthBar();
    }

    private setInitialPose(): void {
        this.sprite.setFrame(18); // IDLE Sul padrão
    }

    public move(worldPos: Position, duration: number): void {
        const dx = worldPos.x - this.x;
        const dy = worldPos.y - this.y;

        // Cancela o timer de parar de andar se houver um novo movimento em seguida
        if (this.stopWalkingTimer) {
            this.stopWalkingTimer.destroy();
            this.stopWalkingTimer = undefined;
        }

        // Cuida da própria animação
        const animKey = this.getDirectionAnimation(dx, dy);
        if (animKey) {
            this.facingDirection = animKey.replace('walk-', '');
            if (!this.isAttacking) {
                if (this.sprite.texture.key !== 'hero') {
                    this.sprite.setTexture('hero');
                }
                this.sprite.play(animKey, true);
            }
        }


        // Limpa tweens anteriores
        this.scene.tweens.killTweensOf(this);
        this.scene.tweens.killTweensOf(this.nameText);


        // Move o container do player
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
                this.stopWalkingTimer = this.scene.time.delayedCall(100, () => {
                    this.stopWalking();
                });
            }
        });

        // Move o texto do nome junto, mantendo offset vertical
        this.scene.tweens.add({
            targets: this.nameText,
            x: worldPos.x,
            y: worldPos.y - this.nameTextOffsetY,
            duration: duration,
            ease: 'Linear'
        });
    }

    private getDirectionAnimation(dx: number, dy: number): string {
        if (dx === 0 && dy === 0) return '';
        if (Math.abs(dx) > Math.abs(dy)) return dx > 0 ? 'walk-east' : 'walk-west';
        return dy > 0 ? 'walk-south' : 'walk-north';
    }

    public playAttackAnimation(targetX: number, targetY: number): void {
        const dx = targetX - this.x;
        const dy = targetY - this.y;

        // Determina direção do ataque
        let animSuffix = this.facingDirection;
        if (Math.abs(dx) > 10 || Math.abs(dy) > 10) {
            if (Math.abs(dx) > Math.abs(dy)) {
                animSuffix = dx > 0 ? 'east' : 'west';
            } else {
                animSuffix = dy > 0 ? 'south' : 'north';
            }
            this.facingDirection = animSuffix;
        }

        const animKey = `attack-${animSuffix}`;
        this.isAttacking = true;

        // Troca para o spritesheet de ataque
        this.sprite.setTexture('hero_slash');
        this.sprite.play(animKey, true);

        // Volta ao normal no fim
        this.sprite.once('animationcomplete', (animation: any) => {
            if (animation.key === animKey) {
                this.isAttacking = false;
                this.sprite.setTexture('hero', this.getIdleFrame(this.facingDirection));
                this.stopWalkingTimer = this.scene.time.delayedCall(100, () => {
                    this.stopWalking();
                });
            }
        });
    }

    private getIdleFrame(direction: string): number {
        if (direction.includes('north')) return 0;
        if (direction.includes('west')) return 9;
        if (direction.includes('east')) return 27;
        return 18; // Default sul
    }

    private stopWalking(): void {
        if (this.isAttacking) return;

        this.sprite.stop();
        const idleFrame = this.getIdleFrame(this.facingDirection);
        this.sprite.setFrame(idleFrame);
    }

    // Sobrescreve o destroy para garantir a limpeza da memória
    public destroy(fromScene?: boolean): void {
        this.sprite.destroy();
        this.nameText.destroy();
        super.destroy(fromScene);
    }
}