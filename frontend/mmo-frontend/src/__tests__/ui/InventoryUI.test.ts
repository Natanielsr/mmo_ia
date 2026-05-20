import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { InventoryUI } from '../../ui/InventoryUI';
import type { ItemData } from '../../types';

describe('InventoryUI', () => {
    let ui: InventoryUI;

    beforeEach(() => {
        document.body.innerHTML = '';
        ui = new InventoryUI();
    });

    afterEach(() => {
        ui.destroy();
    });

    it('render — cria 20 divs de slot', () => {
        expect(document.querySelectorAll('.inv-slot').length).toBe(20);
    });

    it('render — slots começam vazios', () => {
        document.querySelectorAll('.inv-slot').forEach(slot => {
            expect(slot.innerHTML).toBe('');
        });
    });

    it('updateSlot — preenche slot com ícone e nome do item', () => {
        const item: ItemData = { id: 'p1', name: 'Poção de Cura', position: { x: 0, y: 0 }, type: 'Potion' };
        ui.updateSlot(0, item);
        const slot = document.querySelectorAll('.inv-slot')[0];
        expect(slot.textContent).toContain('Poção de Cura');
    });

    it('updateSlot — slot de Weapon exibe classe de ícone de espada', () => {
        const item: ItemData = { id: 'w1', name: 'Espada', position: { x: 0, y: 0 }, type: 'Weapon' };
        ui.updateSlot(0, item);
        const icon = document.querySelectorAll('.inv-slot')[0].querySelector('.item-icon');
        expect(icon?.classList.contains('icon-sword')).toBe(true);
    });

    it('clearSlot — remove item do slot', () => {
        const item: ItemData = { id: 'p1', name: 'Poção', position: { x: 0, y: 0 }, type: 'Potion' };
        ui.updateSlot(0, item);
        ui.clearSlot(0);
        expect(document.querySelectorAll('.inv-slot')[0].innerHTML).toBe('');
    });

    it('toggle — exibe painel quando oculto', () => {
        const panel = document.getElementById('inventory-panel')!;
        expect(panel.style.display).toBe('none');
        ui.toggle();
        expect(panel.style.display).not.toBe('none');
    });

    it('toggle — oculta painel quando visível', () => {
        ui.toggle();
        ui.toggle();
        expect(document.getElementById('inventory-panel')!.style.display).toBe('none');
    });

    it('onSlotClick — callback dispara com índice do slot', () => {
        const onClick = vi.fn();
        ui.onSlotClick = onClick;
        (document.querySelectorAll('.inv-slot')[3] as HTMLElement).click();
        expect(onClick).toHaveBeenCalledWith(3);
    });

    it('onSlotRightClick — callback dispara com id do item', () => {
        const item: ItemData = { id: 'w1', name: 'Espada', position: { x: 0, y: 0 }, type: 'Weapon' };
        ui.updateSlot(5, item);
        const onRightClick = vi.fn();
        ui.onSlotRightClick = onRightClick;
        const slot = document.querySelectorAll('.inv-slot')[5] as HTMLElement;
        slot.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true }));
        expect(onRightClick).toHaveBeenCalledWith('w1');
    });

    it('renderItems — distribui itens nos slots em ordem, limpa os restantes', () => {
        const items: ItemData[] = [
            { id: 'i1', name: 'Item1', position: { x: 0, y: 0 }, type: 'Potion' },
            { id: 'i2', name: 'Item2', position: { x: 0, y: 0 }, type: 'Weapon' },
        ];
        ui.renderItems(items);
        const slots = document.querySelectorAll('.inv-slot');
        expect(slots[0].textContent).toContain('Item1');
        expect(slots[1].textContent).toContain('Item2');
        expect(slots[2].innerHTML).toBe('');
    });
});
