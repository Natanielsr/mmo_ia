import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { DragDropHandler } from '../../ui/DragDropHandler';
import type { ItemData, EquipmentSlot } from '../../types';

if (typeof DragEvent === 'undefined') {
    (globalThis as Record<string, unknown>).DragEvent = class extends MouseEvent {
        dataTransfer: DataTransfer | null;
        constructor(type: string, init: DragEventInit = {}) {
            super(type, init);
            this.dataTransfer = init.dataTransfer ?? null;
        }
    };
}

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

describe('DragDropHandler — setup DOM interactions', () => {
    let handler: DragDropHandler;
    let onEquip: (itemId: string) => void;
    let onUnequip: (slot: EquipmentSlot) => void;
    let onMove: (itemId: string, toIndex: number) => void;

    function makeDT(): DataTransfer {
        return { effectAllowed: '', setData: vi.fn(), getData: vi.fn() } as unknown as DataTransfer;
    }

    beforeEach(() => {
        document.body.innerHTML = '';

        const invPanel = document.createElement('div');
        invPanel.id = 'inventory-panel';
        for (let i = 0; i < 3; i++) {
            const s = document.createElement('div');
            s.className = 'inv-slot';
            s.dataset.index = String(i);
            invPanel.appendChild(s);
        }

        const eqPanel = document.createElement('div');
        eqPanel.id = 'equipment-panel';
        for (const name of ['Weapon', 'Helmet', 'Chest']) {
            const s = document.createElement('div');
            s.className = 'eq-slot';
            s.dataset.slot = name;
            eqPanel.appendChild(s);
        }

        document.body.appendChild(invPanel);
        document.body.appendChild(eqPanel);

        onEquip   = vi.fn();
        onUnequip = vi.fn();
        onMove    = vi.fn();
        handler   = new DragDropHandler(onEquip, onUnequip, onMove);
        handler.setup({} as never, {} as never);
    });

    afterEach(() => { document.body.innerHTML = ''; });

    function startDragFrom(itemId: string, itemType: string): HTMLElement {
        const slot = document.querySelector<HTMLElement>('.inv-slot')!;
        slot.dataset.itemId   = itemId;
        slot.dataset.itemType = itemType;
        slot.dispatchEvent(new DragEvent('dragstart', { bubbles: true, cancelable: true, dataTransfer: makeDT() }));
        return slot;
    }

    it('dragenter válido → adiciona drag-over no eq-slot', () => {
        startDragFrom('w1', 'Weapon');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.dispatchEvent(new DragEvent('dragenter', { bubbles: true, cancelable: true }));
        expect(weaponSlot.classList.contains('drag-over')).toBe(true);
    });

    it('dragenter inválido → NÃO adiciona drag-over (Armor → Weapon slot)', () => {
        startDragFrom('a1', 'Armor');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.dispatchEvent(new DragEvent('dragenter', { bubbles: true, cancelable: true }));
        expect(weaponSlot.classList.contains('drag-over')).toBe(false);
    });

    it('dragleave → remove drag-over do eq-slot', () => {
        startDragFrom('w1', 'Weapon');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.dispatchEvent(new DragEvent('dragenter', { bubbles: true, cancelable: true }));
        weaponSlot.dispatchEvent(new DragEvent('dragleave', { bubbles: true, cancelable: true, relatedTarget: document.body }));
        expect(weaponSlot.classList.contains('drag-over')).toBe(false);
    });

    it('dragstart → adiciona classe dragging ao slot origem', () => {
        const slot = startDragFrom('w1', 'Weapon');
        expect(slot.classList.contains('dragging')).toBe(true);
    });

    it('dragend → remove classe dragging do slot origem', () => {
        const slot = startDragFrom('w1', 'Weapon');
        expect(slot.classList.contains('dragging')).toBe(true);
        document.getElementById('inventory-panel')!.dispatchEvent(new DragEvent('dragend', { bubbles: true }));
        expect(slot.classList.contains('dragging')).toBe(false);
    });

    it('dragend → limpa drag-over de todos os slots', () => {
        startDragFrom('w1', 'Weapon');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.classList.add('drag-over');
        document.getElementById('inventory-panel')!.dispatchEvent(new DragEvent('dragend', { bubbles: true }));
        expect(weaponSlot.classList.contains('drag-over')).toBe(false);
    });

    it('drop no Weapon slot → chama onEquip e remove drag-over', () => {
        startDragFrom('w1', 'Weapon');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.classList.add('drag-over');
        weaponSlot.dispatchEvent(new DragEvent('drop', { bubbles: false, cancelable: true }));
        expect(vi.mocked(onEquip)).toHaveBeenCalledWith('w1');
        expect(weaponSlot.classList.contains('drag-over')).toBe(false);
    });

    it('drop errado (Armor → Weapon slot) → NÃO chama onEquip', () => {
        startDragFrom('a1', 'Armor');
        const weaponSlot = document.querySelector<HTMLElement>('[data-slot="Weapon"]')!;
        weaponSlot.dispatchEvent(new DragEvent('drop', { bubbles: false, cancelable: true }));
        expect(vi.mocked(onEquip)).not.toHaveBeenCalled();
    });
});
