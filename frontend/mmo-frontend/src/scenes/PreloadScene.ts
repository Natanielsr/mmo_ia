import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
    constructor() {
        super({ key: 'PreloadScene' });
    }

    preload() {
        // Carrega spritesheet do personagem
        this.load.spritesheet('hero', 'assets/walkcycle/BODY_male.png', {
            frameWidth: 64,
            frameHeight: 64
        });

        this.load.spritesheet('hero_slash', 'assets/slash/BODY_human.png', {
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
        this.load.image('potion', 'assets/potion.png');

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
            frames: this.anims.generateFrameNumbers('hero', { start: 1, end: 8 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para oeste
        this.anims.create({
            key: 'walk-west',
            frames: this.anims.generateFrameNumbers('hero', { start: 10, end: 17 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para sul
        this.anims.create({
            key: 'walk-south',
            frames: this.anims.generateFrameNumbers('hero', { start: 19, end: 26 }),
            frameRate: 16,
            repeat: -1
        });

        // Animação andando para leste
        this.anims.create({
            key: 'walk-east',
            frames: this.anims.generateFrameNumbers('hero', { start: 28, end: 35 }),
            frameRate: 16,
            repeat: -1
        });

        // --- ANIMAÇÕES DE ATAQUE (SLASH) ---
        // Norte
        this.anims.create({
            key: 'attack-north',
            frames: this.anims.generateFrameNumbers('hero_slash', { start: 0, end: 5 }),
            frameRate: 24,
            repeat: 0
        });

        // Oeste
        this.anims.create({
            key: 'attack-west',
            frames: this.anims.generateFrameNumbers('hero_slash', { start: 6, end: 11 }),
            frameRate: 24,
            repeat: 0
        });

        // Sul
        this.anims.create({
            key: 'attack-south',
            frames: this.anims.generateFrameNumbers('hero_slash', { start: 12, end: 17 }),
            frameRate: 24,
            repeat: 0
        });

        // Leste
        this.anims.create({
            key: 'attack-east',
            frames: this.anims.generateFrameNumbers('hero_slash', { start: 18, end: 23 }),
            frameRate: 24,
            repeat: 0
        });
    }
}