import Phaser from 'phaser';
import type { Position } from '../types';

export class Player extends Phaser.GameObjects.Container {
    public id: string;
    public name: string;
    private sprite: Phaser.GameObjects.Sprite;
    private nameText: Phaser.GameObjects.Text;
    private nameTextOffsetY: number;
    public gridPosition: Position; // Renamed from serverPosition
    public worldPosition: Position;

    constructor(
        id: string,
        name: string,
        gridPosition: Position, // Renamed from serverPosition
        worldPosition: Position,
        scene: Phaser.Scene,
        gridSize: number) {
        super(scene, worldPosition.x, worldPosition.y);
        this.id = id;
        this.worldPosition = worldPosition
        this.gridPosition = gridPosition; // Updated
        this.name = name;

        // 1. O Sprite fica na coordenada (0,0) local do Container
        this.sprite = scene.add.sprite(0, 0, 'hero', 0);
        this.sprite.setDisplaySize(gridSize, gridSize);




        // 2. Texto do nome fica FORA do container para controlar depth global da cena
        this.nameTextOffsetY = (gridSize / 2) + 10;
        this.nameText = scene.add.text(worldPosition.x, worldPosition.y - this.nameTextOffsetY, name, {
            fontSize: '14px', color: '#fff', fontFamily: 'Inter'
        }).setOrigin(0.5);
        this.nameText.setDepth(10000); // acima de tudo



        // 3. Adiciona apenas o sprite como filho do Container
        this.add([this.sprite]);

        // 4. Registra este Container na Cena para ser renderizado
        scene.add.existing(this);



        // 5. Container do player acima dos objetos do mapa
        this.setDepth(1000);

        // Define a pose inicial
        this.setInitialPose();
    }

    private setInitialPose(): void {
        const startAnim = this.scene.anims.get('walk-south');
        if (startAnim && startAnim.frames[1]) {
            this.sprite.setFrame(startAnim.frames[1].textureFrame);
        }
    }

    public move(worldPos: Position, duration: number): void {
        const dx = worldPos.x - this.x;
        const dy = worldPos.y - this.y;

        // Cuida da própria animação
        const animKey = this.getDirectionAnimation(dx, dy);
        if (animKey) {
            this.sprite.play(animKey, true);
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
            onComplete: () => this.stopWalking()
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

    private stopWalking(): void {
        this.sprite.stop();
        if (this.sprite.anims.currentAnim) {
            // Usa o frame de índice 1 (do meio) como frame de parada (idle)
            const frameParado = this.sprite.anims.currentAnim.frames[1]?.textureFrame;
            if (frameParado !== undefined) {
                this.sprite.setFrame(frameParado);
            }
        }
    }

    // Sobrescreve o destroy para garantir a limpeza da memória
    public destroy(fromScene?: boolean): void {
        this.sprite.destroy();
        this.nameText.destroy();
        super.destroy(fromScene);
    }
}