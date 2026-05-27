import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('phaser', () => ({
    default: {
        GameObjects: {
            Sprite: class {
                x = 0; y = 0; scaleX = 1; scaleY = 1; alpha = 1;
                scene: unknown;
                constructor(scene: unknown, x = 0, y = 0, _key = '') {
                    this.scene = scene; this.x = x; this.y = y;
                }
                setDisplaySize() { return this; }
                setDepth() { return this; }
                destroy() {}
            },
        },
    },
}));

import { ItemManager } from '../../managers/ItemManager';
import { makeScene } from '../mocks/scene';
import type { ItemData } from '../../types';

function makeItem(overrides: Partial<ItemData> = {}): ItemData {
    return {
        id: 'item-1',
        name: 'Test Item',
        tagName: 'potion',
        position: { x: 3, y: 4 },
        type: 'Potion',
        ...overrides,
    };
}

describe('ItemManager.syncItem', () => {
    let manager: ItemManager;

    beforeEach(() => {
        manager = new ItemManager(makeScene() as never);
    });

    it('cria HealingPotion para tipo Potion', () => {
        manager.syncItem(makeItem({ type: 'Potion' }));
        expect(manager.getItemsDict().size).toBe(1);
        expect(manager.getItemsDict().get('item-1')).toBeDefined();
    });

    it('cria WorldWeapon para tipo Weapon', () => {
        manager.syncItem(makeItem({ id: 'w1', type: 'Weapon', attackBonus: 5 }));
        expect(manager.getItemsDict().size).toBe(1);
        expect(manager.getItemsDict().get('w1')).toBeDefined();
    });

    it('cria WorldArmor para tipo Armor', () => {
        manager.syncItem(makeItem({ id: 'a1', type: 'Armor', defenseBonus: 3 }));
        expect(manager.getItemsDict().size).toBe(1);
        expect(manager.getItemsDict().get('a1')).toBeDefined();
    });

    it('ignora tipo desconhecido sem lançar erro', () => {
        expect(() => manager.syncItem(makeItem({ type: 'Unknown' }))).not.toThrow();
        expect(manager.getItemsDict().size).toBe(0);
    });

    it('não cria duplicata ao sincronizar o mesmo id duas vezes', () => {
        manager.syncItem(makeItem());
        manager.syncItem(makeItem());
        expect(manager.getItemsDict().size).toBe(1);
    });
});

describe('ItemManager.removeItem', () => {
    it('chama collect() no item e remove do mapa', () => {
        const manager = new ItemManager(makeScene() as never);
        manager.syncItem(makeItem({ type: 'Potion' }));

        const item = manager.getItemsDict().get('item-1')!;
        const collectSpy = vi.spyOn(item, 'collect');

        manager.removeItem('item-1');

        expect(collectSpy).toHaveBeenCalled();
        expect(manager.getItemsDict().has('item-1')).toBe(false);
    });

    it('é no-op quando id não existe', () => {
        const manager = new ItemManager(makeScene() as never);
        expect(() => manager.removeItem('ghost')).not.toThrow();
    });
});

describe('ItemManager.clear', () => {
    it('destrói todos os itens e esvazia o mapa', () => {
        const manager = new ItemManager(makeScene() as never);
        manager.syncItem(makeItem({ id: 'p1', type: 'Potion' }));
        manager.syncItem(makeItem({ id: 'w1', type: 'Weapon' }));
        manager.syncItem(makeItem({ id: 'a1', type: 'Armor' }));

        const items = [...manager.getItemsDict().values()];
        const destroySpies = items.map(item => vi.spyOn(item, 'destroy'));

        manager.clear();

        destroySpies.forEach(spy => expect(spy).toHaveBeenCalled());
        expect(manager.getItemsDict().size).toBe(0);
    });
});
