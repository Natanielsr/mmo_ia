import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
    constructor() {
        super({ key: 'PreloadScene' });
    }

    preload() {
        // Carrega spritesheet do personagem
        this.load.spritesheet('hero', 'assets/human_base_16x18.png', {
            frameWidth: 16,
            frameHeight: 18
        });

        // Futuro: outros assets podem ser carregados aqui
        this.load.image('rat', 'assets/rat.png');
        this.load.image('wolf', 'assets/wolf.png');
        this.load.image('orc', 'assets/orc.png');
        this.load.image('spider', 'assets/spider.png');
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
            frames: this.anims.generateFrameNumbers('hero', { start: 0, end: 2 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para leste
        this.anims.create({
            key: 'walk-east',
            frames: this.anims.generateFrameNumbers('hero', { start: 9, end: 11 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para sul
        this.anims.create({
            key: 'walk-south',
            frames: this.anims.generateFrameNumbers('hero', { start: 18, end: 20 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para oeste
        this.anims.create({
            key: 'walk-west',
            frames: this.anims.generateFrameNumbers('hero', { start: 27, end: 29 }),
            frameRate: 16,
            repeat: -1
        });
    }
}