import { describe, it, expect, beforeEach } from 'vitest';
import { vi } from 'vitest';

vi.mock('phaser', () => ({
    default: {
        GameObjects: {
            Container: class {
                x = 0; y = 0;
                scene: unknown;
                constructor(scene: unknown, x = 0, y = 0) { this.scene = scene; this.x = x; this.y = y; }
                add(_c: unknown) { return this; }
                bringToTop(_c: unknown) { return this; }
                setDepth(_d: number) { return this; }
                destroy(_f?: boolean) {}
            },
        },
    },
}));

import { Player } from '../../entities/Player';
import { makeScene } from '../mocks/scene';
import type { PlayerData } from '../../types';

const BASE_DATA = {
    id: 'p1',
    name: 'Hero',
    position: { x: 2, y: 3 },
    hp: 100,
    maxHp: 100,
    level: 1,
    experience: 0,
    isDead: false,
} as unknown as PlayerData;

const WORLD_POS = { x: 160, y: -224 };

function makePlayer(overrides: Partial<PlayerData> = {}): Player {
    return new Player('p1', 'Hero', { ...BASE_DATA, ...overrides }, WORLD_POS, makeScene() as never, 64);
}

describe('Player — constructor', () => {
    it('inicializa hp, maxHp, level e experience', () => {
        const p = makePlayer();
        expect(p.hp).toBe(100);
        expect(p.maxHp).toBe(100);
        expect(p.level).toBe(1);
        expect(p.experience).toBe(0);
        expect(p.isDead).toBe(false);
    });

    it('suporta casing flexível (PascalCase do backend)', () => {
        const data = { id: 'p1', name: 'Hero', position: { x: 0, y: 0 }, Hp: 80, MaxHp: 120, Level: 3, Experience: 2500 } as unknown as PlayerData;
        const p = new Player('p1', 'Hero', data, WORLD_POS, makeScene() as never, 64);
        expect(p.hp).toBe(80);
        expect(p.maxHp).toBe(120);
        expect(p.level).toBe(3);
        expect(p.experience).toBe(2500);
    });

    it('usa defaults quando propriedades estão ausentes', () => {
        const data = { id: 'p1', name: 'Hero', position: { x: 0, y: 0 } } as unknown as PlayerData;
        const p = new Player('p1', 'Hero', data, WORLD_POS, makeScene() as never, 64);
        expect(p.hp).toBe(100);
        expect(p.maxHp).toBe(100);
        expect(p.level).toBe(1);
        expect(p.experience).toBe(0);
        expect(p.isDead).toBe(false);
    });
});

describe('Player.takeDamage', () => {
    let player: Player;
    beforeEach(() => { player = makePlayer(); });

    it('reduz hp pelo valor do dano', () => {
        player.takeDamage(25);
        expect(player.hp).toBe(75);
    });

    it('clamp: hp nunca fica abaixo de 0', () => {
        player.takeDamage(999);
        expect(player.hp).toBe(0);
    });
});

describe('Player.die', () => {
    it('marca isDead como true', () => {
        const p = makePlayer();
        p.die();
        expect(p.isDead).toBe(true);
    });
});

describe('Player.updateLevelAndXP', () => {
    it('atualiza level e experience', () => {
        const p = makePlayer();
        p.updateLevelAndXP(2, 1500);
        expect(p.level).toBe(2);
        expect(p.experience).toBe(1500);
    });

    it('não lança erro ao subir de nível (level up effect usa tweens)', () => {
        const p = makePlayer();
        expect(() => p.updateLevelAndXP(2, 1000)).not.toThrow();
        expect(p.level).toBe(2);
    });

    it('não altera level quando o valor é o mesmo', () => {
        const p = makePlayer();
        p.updateLevelAndXP(1, 500);
        expect(p.level).toBe(1);
        expect(p.experience).toBe(500);
    });

    it('atualiza level e experience mesmo quando isDead=true (ramo skull)', () => {
        const p = makePlayer();
        p.die();
        expect(() => p.updateLevelAndXP(1, 300)).not.toThrow();
        expect(p.level).toBe(1);
        expect(p.experience).toBe(300);
    });
});

describe('Player.move', () => {
    it('não lança erro ao mover em qualquer direção', () => {
        const p = makePlayer();
        expect(() => p.move({ x: 224, y: -288 }, 500)).not.toThrow();
    });
});

describe('Player.playAttackAnimation', () => {
    it('define isAttacking=true', () => {
        const p = makePlayer();
        p.playAttackAnimation(200, 100);
        expect(p.isAttacking).toBe(true);
    });

    it('funciona com alvo na diagonal', () => {
        const p = makePlayer();
        expect(() => p.playAttackAnimation(100, 100)).not.toThrow();
    });
});

describe('Player.destroy', () => {
    it('não lança erro ao destruir', () => {
        const p = makePlayer();
        expect(() => p.destroy()).not.toThrow();
    });
});

describe('Player.setEquippedWeaponOverlay', () => {
    it('null não cria sprite extra', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        const callsBefore = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls.length;

        p.setEquippedWeaponOverlay(null, null);

        expect((scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls.length).toBe(callsBefore);
    });

    it('walkKey válida cria weaponSprite com frame idle', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);

        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');

        expect(scene.add.sprite).toHaveBeenCalledWith(0, 0, 'weapon_dagger_walk', expect.any(Number));
    });

    it('chamar duas vezes com mesma key não cria sprite duplicado', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        const callsBefore = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls.length;

        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');

        expect((scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls.length).toBe(callsBefore + 1);
    });

    it('null após weapon definida destrói o sprite', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const calls = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results;
        const weaponSprite = calls[calls.length - 1].value;

        p.setEquippedWeaponOverlay(null, null);

        expect(weaponSprite.destroy).toHaveBeenCalled();
    });
});

describe('Player.playAttackAnimation — weapon overlay', () => {
    it('com arma equipada: mostra e anima weaponSprite', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;

        p.playAttackAnimation(0, 50); // south

        expect(weaponSprite.setVisible).toHaveBeenCalledWith(true);
        expect(weaponSprite.play).toHaveBeenCalledWith('weapon_dagger_slash-south', true);
    });

    it('sem arma equipada: não lança erro', () => {
        const p = makePlayer();
        expect(() => p.playAttackAnimation(0, 50)).not.toThrow();
    });

    it('com arma equipada: east quando dx > dy', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;

        // WORLD_POS = (160, -224); target (1000, -224) → dx=840, dy=0 → east
        p.playAttackAnimation(1000, -224);

        expect(weaponSprite.play).toHaveBeenCalledWith('weapon_dagger_slash-east', true);
    });
});

describe('Player overlay z-order', () => {
    it('weapon fica por cima do body overlay (bringToTop chamado ao equipar weapon)', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedBodyOverlay('Torso', 'armor_walk', 'armor_slash');
        const bringToTopSpy = vi.spyOn(p, 'bringToTop');

        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');

        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        expect(bringToTopSpy).toHaveBeenCalledWith(weaponSprite);
    });

    it('weapon permanece por cima quando body overlay é equipado depois', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        const bringToTopSpy = vi.spyOn(p, 'bringToTop');

        p.setEquippedBodyOverlay('Torso', 'armor_walk', 'armor_slash');

        expect(bringToTopSpy).toHaveBeenCalledWith(weaponSprite);
    });
});

describe('Player.move — weapon overlay walk', () => {
    it('não chama setTexture ao andar com arma equipada (evita flash de frame 0)', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        const setTextureSpy = vi.spyOn(weaponSprite, 'setTexture');

        p.move({ x: WORLD_POS.x, y: WORLD_POS.y + 64 }, 500); // south

        expect(setTextureSpy).not.toHaveBeenCalled();
    });

    it('chama play no weapon sprite com anim walk correta ao mover', () => {
        const scene = makeScene();
        const p = new Player('p1', 'Hero', BASE_DATA, WORLD_POS, scene as never, 64);
        p.setEquippedWeaponOverlay('weapon_dagger_walk', 'weapon_dagger_slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;

        p.move({ x: WORLD_POS.x, y: WORLD_POS.y + 64 }, 500); // south

        expect(weaponSprite.play).toHaveBeenCalledWith('weapon_dagger_walk-south', true);
    });
});
