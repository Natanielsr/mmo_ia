import { describe, it, expect, beforeEach, vi } from 'vitest';
import { DragDropHandler } from '../../ui/DragDropHandler';
import type { ItemData, EquipmentSlot } from '../../types';

const weapon: ItemData = { id: 'w1', name: 'Espada', position: { x: 0, y: 0 }, type: 'Weapon', attackBonus: 5 };
const armor: ItemData  = { id: 'a1', name: 'Armadura', position: { x: 0, y: 0 }, type: 'Armor', defenseBonus: 3 };

describe('DragDropHandler', () => {
    let onEquip: (itemId: string) => void;
    let onUnequip: (slot: EquipmentSlot) => void;
    let onMoveInInventory: (itemId: string, toIndex: number) => void;
    let handler: DragDropHandler;

    beforeEach(() => {
        onEquip           = vi.fn();
        onUnequip         = vi.fn();
        onMoveInInventory = vi.fn();
        handler = new DragDropHandler(onEquip, onUnequip, onMoveInInventory);
    });

    // --- canDrop ---

    it('canDrop_InventoryToEquipment — retorna true para arma no slot de arma', () => {
        expect(handler.canDrop(weapon, 'Weapon')).toBe(true);
    });

    it('canDrop_InventoryToEquipment — retorna false para armadura no slot de arma', () => {
        expect(handler.canDrop(armor, 'Weapon')).toBe(false);
    });

    it('canDrop_EquipmentToInventory — sempre retorna true para qualquer item', () => {
        expect(handler.canDrop(weapon, 'inventory')).toBe(true);
        expect(handler.canDrop(armor, 'inventory')).toBe(true);
    });

    it('canDrop_InventoryToInventory — sempre retorna true', () => {
        expect(handler.canDrop(armor, 'inventory')).toBe(true);
    });

    // --- handleDrop ---

    it('handleDrop — dispara onEquip ao soltar em zona de equipamento', () => {
        handler.handleDrop(weapon, 'Weapon');
        expect(vi.mocked(onEquip)).toHaveBeenCalledWith('w1');
        expect(vi.mocked(onMoveInInventory)).not.toHaveBeenCalled();
    });

    it('handleDrop — dispara onMoveInInventory ao soltar em zona de inventário', () => {
        handler.handleDrop(weapon, 'inventory', 3);
        expect(vi.mocked(onMoveInInventory)).toHaveBeenCalledWith('w1', 3);
        expect(vi.mocked(onEquip)).not.toHaveBeenCalled();
    });
});
