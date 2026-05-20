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
                _emit(event: string) { this._listeners[event]?.forEach(fn => fn()); }
                destroy(_f?: boolean) {}
            },
        },
    },
}));

import { MonsterManager } from '../../managers/MonsterManager';
import { makeScene } from '../mocks/scene';
import type { MonsterData } from '../../types';

const M1: MonsterData = {
    id: 'm1',
    name: 'Goblin',
    objectCode: 'goblin',
    position: { x: 5, y: 5 },
    hp: 80,
    maxHp: 80,
    attackPower: 8,
    isDead: false,
};

describe('MonsterManager — estado inicial', () => {
    it('getMonstersDict retorna dicionário vazio', () => {
        const mm = new MonsterManager(makeScene() as never);
        expect(mm.getMonstersDict()).toEqual({});
    });

    it('getMonster retorna null para id desconhecido', () => {
        const mm = new MonsterManager(makeScene() as never);
        expect(mm.getMonster('x')).toBeNull();
    });
});

describe('MonsterManager.syncMonster', () => {
    let mm: MonsterManager;
    beforeEach(() => { mm = new MonsterManager(makeScene() as never); });

    it('cria novo monstro quando id não existe', () => {
        mm.syncMonster(M1);
        expect(mm.getMonster('m1')).not.toBeNull();
        expect(mm.getMonster('m1')!.id).toBe('m1');
    });

    it('não cria duplicata — atualiza monstro existente', () => {
        mm.syncMonster(M1);
        mm.syncMonster({ ...M1, hp: 40 });
        const dict = mm.getMonstersDict();
        expect(Object.keys(dict)).toHaveLength(1);
        expect(dict['m1'].hp).toBe(40);
    });

    it('cria múltiplos monstros distintos', () => {
        mm.syncMonster(M1);
        mm.syncMonster({ ...M1, id: 'm2', name: 'Orc' });
        expect(Object.keys(mm.getMonstersDict())).toHaveLength(2);
    });
});

describe('MonsterManager.syncMonsters (lista)', () => {
    it('sincroniza uma lista inteira de monstros', () => {
        const mm = new MonsterManager(makeScene() as never);
        mm.syncMonsters([M1, { ...M1, id: 'm2' }, { ...M1, id: 'm3' }]);
        expect(Object.keys(mm.getMonstersDict())).toHaveLength(3);
    });
});

describe('MonsterManager.monsterDied', () => {
    it('chama die() no monstro e agenda remoção', () => {
        const scene = makeScene();
        const mm = new MonsterManager(scene as never);
        mm.syncMonster(M1);
        mm.monsterDied('m1');

        const monster = mm.getMonster('m1');
        expect(monster!.isDead).toBe(true);
        expect(scene.time.delayedCall).toHaveBeenCalledWith(5000, expect.any(Function));
    });

    it('é no-op para id desconhecido', () => {
        const mm = new MonsterManager(makeScene() as never);
        expect(() => mm.monsterDied('nope')).not.toThrow();
    });

    it('callback de cleanup remove monstro do dicionário', () => {
        const scene = makeScene();
        const mm = new MonsterManager(scene as never);
        mm.syncMonster(M1);
        mm.monsterDied('m1');

        const [, callback] = (scene.time.delayedCall as ReturnType<typeof vi.fn>).mock.calls[0] as [number, () => void];
        callback();

        expect(mm.getMonster('m1')).toBeNull();
    });

    it('callback de cleanup é no-op quando monstro já foi removido antes', () => {
        const scene = makeScene();
        const mm = new MonsterManager(scene as never);
        mm.syncMonster(M1);
        mm.monsterDied('m1');

        // Remove o monstro antes do timer disparar
        mm.removeMonster('m1');

        const [, callback] = (scene.time.delayedCall as ReturnType<typeof vi.fn>).mock.calls[0] as [number, () => void];
        expect(() => callback()).not.toThrow();
        expect(mm.getMonster('m1')).toBeNull();
    });
});

describe('MonsterManager.removeMonster', () => {
    it('remove monstro do dicionário', () => {
        const mm = new MonsterManager(makeScene() as never);
        mm.syncMonster(M1);
        mm.removeMonster('m1');
        expect(mm.getMonster('m1')).toBeNull();
    });

    it('é no-op para id inexistente', () => {
        const mm = new MonsterManager(makeScene() as never);
        expect(() => mm.removeMonster('ghost')).not.toThrow();
    });
});

describe('MonsterManager — callback de ataque ao clicar', () => {
    it('chama onAttackMonster com o id correto ao clicar no monstro', () => {
        const mm = new MonsterManager(makeScene() as never);
        const attackCallback = vi.fn();
        mm.onAttackMonster = attackCallback;
        mm.syncMonster(M1);

        const monster = mm.getMonster('m1');
        (monster as unknown as { _emit: (e: string) => void })._emit('pointerdown');

        expect(attackCallback).toHaveBeenCalledWith('m1');
    });

    it('não lança erro se onAttackMonster não está definido', () => {
        const mm = new MonsterManager(makeScene() as never);
        mm.syncMonster(M1);
        const monster = mm.getMonster('m1');
        expect(() => {
            (monster as unknown as { _emit: (e: string) => void })._emit('pointerdown');
        }).not.toThrow();
    });
});
