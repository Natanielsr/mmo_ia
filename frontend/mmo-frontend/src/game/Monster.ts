import Phaser from 'phaser';
import type { Position, MonsterData } from '../types';

const GRID_SIZE = 64;

export class Monster extends Phaser.GameObjects.Container {
    public id: string;
    public name: string;
    public objectCode: string;
    public sprite: Phaser.GameObjects.Sprite;
    public nameText: Phaser.GameObjects.Text;
    private hpBar: Phaser.GameObjects.Graphics;
    public gridPosition: Position; // Renamed from serverPosition
    public hp: number;
    public maxHp: number;
    public isDead: boolean;

    constructor(
        monsterData: MonsterData,
        scene: Phaser.Scene
    ) {
        const worldPos = Monster.getWorldCoordinates(monsterData.position);
        super(scene, worldPos.x, worldPos.y);

        this.id = monsterData.id;
        this.name = monsterData.name;
        this.objectCode = monsterData.objectCode;
        this.gridPosition = monsterData.position; // Updated
        this.hp = monsterData.hp;
        this.maxHp = monsterData.maxHp;
        this.isDead = monsterData.isDead;

        // 1. Sprite do monstro (placeholder - será melhorado com sprites reais)
        this.sprite = scene.add.sprite(0, 0, 'hero', 0);
        this.sprite.setDisplaySize(GRID_SIZE, GRID_SIZE);
        this.sprite.setTint(0xFF0000); // Vermelho para diferenciar de players

        // 2. Barra de vida
        this.hpBar = scene.add.graphics();
        this.updateHpBar();

        // 3. Nome do monstro
        const textOffsetY = (GRID_SIZE / 2) + 15;
        this.nameText = scene.add.text(0, -textOffsetY, `${this.name} (${this.hp}/${this.maxHp})`, {
            fontSize: '12px',
            color: '#fff',
            fontFamily: 'Inter',
            backgroundColor: 'rgba(0, 0, 0, 0.6)',
            padding: { left: 4, right: 4, top: 2, bottom: 2 }
        }).setOrigin(0.5);

        // 4. Adiciona todos ao container
        this.add([this.sprite, this.hpBar, this.nameText]);

        // 5. Registra o container na cena
        scene.add.existing(this);

        // 6. Define profundidade para ficar acima dos objetos do mapa, mas abaixo dos players
        this.setDepth(500);
    }

    private updateHpBar(): void {
        this.hpBar.clear();

        const barWidth = 40;
        const barHeight = 6;
        const barX = -barWidth / 2;
        const barY = -GRID_SIZE / 2 - 10;

        // Fundo da barra (preto)
        this.hpBar.fillStyle(0x000000, 0.8);
        this.hpBar.fillRect(barX, barY, barWidth, barHeight);

        // Barra de vida atual (verde)
        const hpPercent = Math.max(0, this.hp) / this.maxHp;
        const currentWidth = barWidth * hpPercent;

        let barColor: number;
        if (hpPercent > 0.6) barColor = 0x00FF00; // Verde
        else if (hpPercent > 0.3) barColor = 0xFFFF00; // Amarelo
        else barColor = 0xFF0000; // Vermelho

        this.hpBar.fillStyle(barColor, 1);
        this.hpBar.fillRect(barX, barY, currentWidth, barHeight);

        // Borda da barra
        this.hpBar.lineStyle(1, 0xFFFFFF, 0.8);
        this.hpBar.strokeRect(barX, barY, barWidth, barHeight);
    }

    public updateFromServer(monsterData: MonsterData): void {
        this.gridPosition = monsterData.position; // Updated
        this.hp = monsterData.hp;
        this.maxHp = monsterData.maxHp;
        this.isDead = monsterData.isDead;

        const worldPos = Monster.getWorldCoordinates(monsterData.position);

        // Move o container para a nova posição
        this.scene.tweens.add({
            targets: this,
            x: worldPos.x,
            y: worldPos.y,
            duration: 500,
            ease: 'Power2'
        });

        // Atualiza o texto do nome com HP atual
        if (!this.isDead) {
            this.nameText.setText(`${this.name} (${this.hp}/${this.maxHp})`);
        } else {
            this.nameText.setText(`${this.name} 💀`);
            this.sprite.setAlpha(0.5);
        }

        // Atualiza a barra de vida
        this.updateHpBar();
    }

    public takeDamage(damage: number): void {
        if (this.isDead) return;

        this.hp = Math.max(0, this.hp - damage);

        // Animação de dano (flash vermelho)
        this.sprite.setTint(0xFF5555);
        this.scene.time.delayedCall(100, () => {
            this.sprite.setTint(0xFF0000);
        });

        // Atualiza visual
        this.updateFromServer({
            id: this.id,
            name: this.name,
            objectCode: this.objectCode,
            position: this.gridPosition, // Updated
            hp: this.hp,
            maxHp: this.maxHp,
            attackPower: 0, // não usado aqui
            isDead: this.hp <= 0
        });
    }

    private static getWorldCoordinates(serverPosition: Position): Position {
        const px = serverPosition.x * GRID_SIZE + (GRID_SIZE / 2);
        const py = -serverPosition.y * GRID_SIZE - (GRID_SIZE / 2);
        return { x: px, y: py };
    }

    public destroy(fromScene?: boolean): void {
        this.sprite.destroy();
        this.hpBar.destroy();
        this.nameText.destroy();
        super.destroy(fromScene);
    }
}