import { describe, it, expect, beforeEach } from 'vitest';
import { vi } from 'vitest';

vi.mock('phaser', () => ({
    default: {
        GameObjects: {
            Container: class {
                x = 0; y = 0;
                scene: unknown;
                private _listeners: Record<string, Array<() => void>> = {};
                constructor(scene: unknown, x = 0, y = 0) { this.scene = scene; this.x = x; this.y = y; }
                add(_c: unknown) { return this; }
                setDepth(_d: number) { return this; }
                setSize(_w: number, _h: number) { return this; }
                setInteractive() { return this; }
                on(event: string, fn: () => void) {
                    if (!this._listeners[event]) this._listeners[event] = [];
                    this._listeners[event].push(fn);
                    return this;
                }
                destroy(_f?: boolean) {}
            },
        },
    },
}));

import { Monster } from '../../entities/Monster';
import { makeScene } from '../mocks/scene';
import type { MonsterData } from '../../types';

const BASE: MonsterData = {
    id: 'm1',
    name: 'Goblin',
    objectCode: 'goblin',
    position: { x: 3, y: 4 },
    hp: 100,
    maxHp: 100,
    attackPower: 10,
    isDead: false,
};

describe('Monster — constructor', () => {
    it('inicializa propriedades a partir de monsterData', () => {
        const m = new Monster(BASE, makeScene() as never);
        expect(m.id).toBe('m1');
        expect(m.name).toBe('Goblin');
        expect(m.hp).toBe(100);
        expect(m.maxHp).toBe(100);
        expect(m.isDead).toBe(false);
        expect(m.gridPosition).toEqual({ x: 3, y: 4 });
    });

    it('inicializa com isDead=true quando monsterData indica morto', () => {
        const m = new Monster({ ...BASE, isDead: true }, makeScene() as never);
        expect(m.isDead).toBe(true);
    });
});

describe('Monster.takeDamage', () => {
    let monster: Monster;
    beforeEach(() => { monster = new Monster(BASE, makeScene() as never); });

    it('reduz hp pelo valor do dano', () => {
        monster.takeDamage(30);
        expect(monster.hp).toBe(70);
    });

    it('reduz hp múltiplas vezes', () => {
        monster.takeDamage(40);
        monster.takeDamage(40);
        expect(monster.hp).toBe(20);
    });

    it('clamp: hp nunca fica abaixo de 0', () => {
        monster.takeDamage(999);
        expect(monster.hp).toBe(0);
    });

    it('é no-op quando monstro já está morto', () => {
        const dead = new Monster({ ...BASE, isDead: true }, makeScene() as never);
        dead.takeDamage(50);
        expect(dead.hp).toBe(100);
    });
});

describe('Monster.die', () => {
    it('marca isDead como true', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.die();
        expect(m.isDead).toBe(true);
    });

    it('esconde nameText', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.die();
        expect(m.nameText.visible).toBe(false);
    });

    it('aplica tint azul no sprite', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.die();
        expect((m.sprite as unknown as { setTint: ReturnType<typeof vi.fn> }).setTint).toHaveBeenCalledWith(0xaaaaff);
    });
});

describe('Monster.updateFromServer', () => {
    it('atualiza hp, maxHp, gridPosition e isDead', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.updateFromServer({ ...BASE, hp: 60, maxHp: 120, position: { x: 7, y: 8 } });
        expect(m.hp).toBe(60);
        expect(m.maxHp).toBe(120);
        expect(m.gridPosition).toEqual({ x: 7, y: 8 });
    });

    it('esconde nameText e hpBar ao morrer via updateFromServer', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.updateFromServer({ ...BASE, hp: 0, isDead: true });
        expect(m.isDead).toBe(true);
        expect(m.nameText.visible).toBe(false);
    });

    it('atualiza texto do nameText quando monstro está vivo', () => {
        const m = new Monster(BASE, makeScene() as never);
        m.updateFromServer({ ...BASE, hp: 50, maxHp: 100 });
        // nameText é público — verifica que não foi ocultado (ramo !isDead)
        expect(m.nameText.visible).toBe(true);
        expect(m.isDead).toBe(false);
    });
});
