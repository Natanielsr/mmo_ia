import { describe, it, expect, vi, beforeEach } from 'vitest';
import { InventoryManager } from '../../managers/InventoryManager';
import type { ItemData } from '../../types';

function makeItem(overrides: Partial<ItemData> = {}): ItemData {
    return {
        id: 'item-1',
        name: 'Health Potion',
        position: { x: 0, y: 0 },
        type: 'Potion',
        ...overrides,
    };
}

describe('InventoryManager', () => {
    let manager: InventoryManager;

    beforeEach(() => {
        manager = new InventoryManager();
    });

    it('addItem — adiciona item ao array', () => {
        manager.addItem(makeItem());
        expect(manager.getItems()).toHaveLength(1);
    });

    it('addItem — não ultrapassa 20 itens', () => {
        for (let i = 0; i < 25; i++) manager.addItem(makeItem({ id: `item-${i}` }));
        expect(manager.getItems()).toHaveLength(20);
    });

    it('removeItem — remove por itemId', () => {
        manager.addItem(makeItem());
        manager.removeItem('item-1');
        expect(manager.getItems()).toHaveLength(0);
    });

    it('getItems — retorna snapshot atual', () => {
        manager.addItem(makeItem({ id: 'a' }));
        manager.addItem(makeItem({ id: 'b' }));
        expect(manager.getItems()).toHaveLength(2);
    });

    it('useItem — dispara callback onUseItem com itemId', () => {
        const cb = vi.fn();
        manager.onUseItem = cb;
        manager.useItem('item-1');
        expect(cb).toHaveBeenCalledWith('item-1');
    });

    it('dropItem — dispara callback onDropItem com itemId e coordenadas', () => {
        const cb = vi.fn();
        manager.onDropItem = cb;
        manager.dropItem('item-1', 5, 10);
        expect(cb).toHaveBeenCalledWith('item-1', 5, 10);
    });

    it('clear — esvazia todos os itens', () => {
        manager.addItem(makeItem({ id: 'a' }));
        manager.addItem(makeItem({ id: 'b' }));
        manager.clear();
        expect(manager.getItems()).toHaveLength(0);
    });

    it('onInventoryUpdated — substitui inventário inteiro pelo payload do servidor', () => {
        manager.addItem(makeItem({ id: 'old' }));
        const newItems = [makeItem({ id: 'new-1' }), makeItem({ id: 'new-2' })];
        manager.onInventoryUpdated(newItems);
        const items = manager.getItems();
        expect(items).toHaveLength(2);
        expect(items[0].id).toBe('new-1');
    });
});
