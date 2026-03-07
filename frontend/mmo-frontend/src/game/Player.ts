import Phaser from 'phaser';

export class Player extends Phaser.GameObjects.Container {
    public id: string;
    private sprite: Phaser.GameObjects.Sprite;
    private nameText: Phaser.GameObjects.Text;

    constructor(scene: Phaser.Scene, x: number, y: number, id: string, gridSize: number) {
        super(scene, x, y);
        this.id = id;

        // 1. O Sprite fica na coordenada (0,0) local do Container
        this.sprite = scene.add.sprite(0, 0, 'hero', 0);
        this.sprite.setDisplaySize(gridSize, gridSize);

        // 2. O Texto fica posicionado relativamente ao Sprite, usando o gridSize
        const textOffsetY = (gridSize / 2) + 10;
        this.nameText = scene.add.text(0, -textOffsetY, id, {
            fontSize: '14px', color: '#fff', fontFamily: 'Inter'
        }).setOrigin(0.5);

        // 3. Adiciona os elementos visuais como filhos deste Container
        this.add([this.sprite, this.nameText]);

        // 4. Registra este Container na Cena para ser renderizado
        scene.add.existing(this);

        // Define a pose inicial
        this.setInitialPose();
    }

    private setInitialPose(): void {
        const startAnim = this.scene.anims.get('walk-south');
        if (startAnim && startAnim.frames[1]) {
            this.sprite.setFrame(startAnim.frames[1].textureFrame);
        }
    }

    public move(targetX: number, targetY: number, duration: number): void {
        const dx = targetX - this.x;
        const dy = targetY - this.y;

        // Cuida da própria animação
        const animKey = this.getDirectionAnimation(dx, dy);
        if (animKey) {
            this.sprite.play(animKey, true);
        }

        // Limpa tweens anteriores DESTE container especificamente
        this.scene.tweens.killTweensOf(this);

        // Move o Container (Sprite e Texto vão juntos automaticamente!)
        this.scene.tweens.add({
            targets: this,
            x: targetX,
            y: targetY,
            duration: duration,
            ease: 'Linear',
            onComplete: () => this.stopWalking()
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