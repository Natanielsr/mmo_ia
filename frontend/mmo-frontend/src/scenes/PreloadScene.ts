import Phaser from 'phaser';
import { WEAPON_OVERLAY_REGISTRY, getAllBodyConfigs } from '../config/overlays';
import { ITEM_ICON_REGISTRY } from '../config/itemIconRegistry';
import { MONSTER_SPRITE_REGISTRY } from '../config/monsterSpriteRegistry';

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

        for (const cfg of Object.values(WEAPON_OVERLAY_REGISTRY)) {
            this.load.spritesheet(cfg.walkTextureKey, cfg.walkAssetPath, {
                frameWidth: cfg.frameWidth,
                frameHeight: cfg.frameHeight
            });
            this.load.spritesheet(cfg.slashTextureKey, cfg.slashAssetPath, {
                frameWidth: cfg.frameWidth,
                frameHeight: cfg.frameHeight
            });
        }

        for (const cfg of getAllBodyConfigs()) {
            this.load.spritesheet(cfg.walkTextureKey, cfg.walkAssetPath, {
                frameWidth: cfg.frameWidth,
                frameHeight: cfg.frameHeight
            });
            this.load.spritesheet(cfg.slashTextureKey, cfg.slashAssetPath, {
                frameWidth: cfg.frameWidth,
                frameHeight: cfg.frameHeight
            });
        }


        // Futuro: outros assets podem ser carregados aqui
        for (const [key, path] of Object.entries(MONSTER_SPRITE_REGISTRY)) {
            this.load.image(key, path);
        }

        this.load.image('tree', 'assets/tree.png');
        this.load.image('rock', 'assets/rock.png');
        this.load.image('bush', 'assets/bush.png');
        this.load.image('pillar', 'assets/pillar.png');
        for (const [tagName, path] of Object.entries(ITEM_ICON_REGISTRY)) {
            this.load.image(tagName, path.replace(/^\//, ''));
        }

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

        // Animações de overlay de armas (geradas automaticamente pelo registry)
        const dirs = ['north', 'west', 'south', 'east'] as const;
        for (const cfg of Object.values(WEAPON_OVERLAY_REGISTRY)) {
            for (const dir of dirs) {
                const [ws, we] = cfg.walkDirectionFrames[dir];
                this.anims.create({
                    key: `${cfg.walkTextureKey}-${dir}`,
                    frames: this.anims.generateFrameNumbers(cfg.walkTextureKey, { start: ws, end: we }),
                    frameRate: cfg.walkFrameRate,
                    repeat: -1
                });
                const [ss, se] = cfg.slashDirectionFrames[dir];
                this.anims.create({
                    key: `${cfg.slashTextureKey}-${dir}`,
                    frames: this.anims.generateFrameNumbers(cfg.slashTextureKey, { start: ss, end: se }),
                    frameRate: cfg.slashFrameRate,
                    repeat: 0
                });
            }
        }

        // Animações de overlay de body (walk + slash por direção)
        for (const cfg of getAllBodyConfigs()) {
            for (const dir of dirs) {
                const [ws, we] = cfg.walkDirectionFrames[dir];
                this.anims.create({
                    key: `${cfg.walkTextureKey}-${dir}`,
                    frames: this.anims.generateFrameNumbers(cfg.walkTextureKey, { start: ws, end: we }),
                    frameRate: cfg.walkFrameRate,
                    repeat: -1
                });
                const [ss, se] = cfg.slashDirectionFrames[dir];
                this.anims.create({
                    key: `${cfg.slashTextureKey}-${dir}`,
                    frames: this.anims.generateFrameNumbers(cfg.slashTextureKey, { start: ss, end: se }),
                    frameRate: cfg.slashFrameRate,
                    repeat: 0
                });
            }
        }
    }
}