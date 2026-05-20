import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { EquipmentUI } from '../../ui/EquipmentUI';
import type { ItemData, EquipmentSlot } from '../../types';

describe('EquipmentUI', () => {
    let ui: EquipmentUI;

    beforeEach(() => {
        document.body.innerHTML = '';
        ui = new EquipmentUI();
    });

    afterEach(() => {
        ui.destroy();
    });

    it('render — cria 6 elementos de slot nomeados', () => {
        const expected: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield'];
        expected.forEach(slot => {
            expect(document.querySelector(`[data-slot="${slot}"]`)).not.toBeNull();
        });
    });

    it('updateSlot — preenche slot com dados do item', () => {
        const item: ItemData = { id: 'w1', name: 'Espada Longa', position: { x: 0, y: 0 }, type: 'Weapon' };
        ui.updateSlot('Weapon', item);
        expect(document.querySelector('[data-slot="Weapon"]')!.textContent).toContain('Espada Longa');
    });

    it('clearSlot — esvazia slot', () => {
        const item: ItemData = { id: 'w1', name: 'Espada', position: { x: 0, y: 0 }, type: 'Weapon' };
        ui.updateSlot('Weapon', item);
        ui.clearSlot('Weapon');
        expect(document.querySelector('[data-slot="Weapon"]')!.innerHTML).toBe('');
    });

    it('toggle — exibe painel quando oculto', () => {
        const panel = document.getElementById('equipment-panel')!;
        expect(panel.style.display).toBe('none');
        ui.toggle();
        expect(panel.style.display).not.toBe('none');
    });

    it('toggle — oculta painel quando visível', () => {
        ui.toggle();
        ui.toggle();
        expect(document.getElementById('equipment-panel')!.style.display).toBe('none');
    });

    it('onSlotClick — dispara callback com nome do EquipmentSlot', () => {
        const onClick = vi.fn();
        ui.onSlotClick = onClick;
        (document.querySelector('[data-slot="Weapon"]') as HTMLElement).click();
        expect(onClick).toHaveBeenCalledWith('Weapon' as EquipmentSlot);
    });

    it('updateStats — atualiza valores exibidos de ataque e defesa', () => {
        ui.updateStats(15, 8);
        expect(document.getElementById('eq-attack')!.textContent).toBe('15');
        expect(document.getElementById('eq-defense')!.textContent).toBe('8');
    });
});
