import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
    constructor() {
        super({ key: 'PreloadScene' });
    }

    preload() {
        // Carrega spritesheet do personagem
        this.load.spritesheet('hero', 'assets/BODY_male.png', {
            frameWidth: 64,
            frameHeight: 64
        });

        // Futuro: outros assets podem ser carregados aqui
        this.load.image('rat', 'assets/rat.png');
        this.load.image('wolf', 'assets/wolf.png');
        this.load.image('orc', 'assets/orc.png');
        this.load.image('spider', 'assets/spider.png');

        this.load.image('tree', 'assets/tree.png');
        this.load.image('rock', 'assets/rock.png');
        this.load.image('bush', 'assets/bush.png');
        this.load.image('pillar', 'assets/pillar.png');

        for (let i = 1; i <= 12; i++) {
            this.load.image(`grass${i}`, `assets/grass${i}.png`);
        }
        // this.load.audio('backgroundMusic', 'assets/music.mp3');
    }

    create() {
        // Cria todas as animações para o personagem
        this.createHeroAnimations();

        // Começa a cena principal
        this.scene.start('MainScene');
    }

    private createHeroAnimations(): void {
        // Animação andando para norte
        this.anims.create({
            key: 'walk-north',
            frames: this.anims.generateFrameNumbers('hero', { start: 0, end: 8 }),
            frameRate: 36,
            repeat: -1
        });

        // Animação andando para oeste
        this.anims.create({
            key: 'walk-west',
            frames: this.anims.generateFrameNumbers('hero', { start: 9, end: 17 }),
            frameRate: 36,
            repeat: -1
        });

        // Animação andando para sul
        this.anims.create({
            key: 'walk-south',
            frames: this.anims.generateFrameNumbers('hero', { start: 18, end: 26 }),
            frameRate: 36,
            repeat: -1
        });

        // Animação andando para leste
        this.anims.create({
            key: 'walk-east',
            frames: this.anims.generateFrameNumbers('hero', { start: 27, end: 35 }),
            frameRate: 36,
            repeat: -1
        });
    }
}