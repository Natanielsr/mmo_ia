import { describe, it, expect, beforeEach } from 'vitest';
import { EquipmentManager } from '../../managers/EquipmentManager';
import type { ItemData, EquipmentSlot } from '../../types';

function makeWeapon(id = 'w1'): ItemData {
    return { id, name: 'Sword', position: { x: 0, y: 0 }, type: 'Weapon', attackBonus: 5 };
}

function makeArmor(id = 'a1'): ItemData {
    return { id, name: 'Vest', position: { x: 0, y: 0 }, type: 'Armor', defenseBonus: 3 };
}

describe('EquipmentManager', () => {
    let manager: EquipmentManager;

    beforeEach(() => {
        manager = new EquipmentManager();
    });

    it('equipItem — armazena item no slot correto', () => {
        manager.equipItem('Weapon', makeWeapon());
        expect(manager.getEquippedItem('Weapon')).not.toBeNull();
    });

    it('equipItem — substituir slot retorna item antigo', () => {
        const first  = makeWeapon('w1');
        const second = makeWeapon('w2');
        manager.equipItem('Weapon', first);
        const displaced = manager.equipItem('Weapon', second);
        expect(displaced?.id).toBe('w1');
        expect(manager.getEquippedItem('Weapon')?.id).toBe('w2');
    });

    it('unequipItem — remove item do slot', () => {
        manager.equipItem('Weapon', makeWeapon());
        const removed = manager.unequipItem('Weapon');
        expect(removed).not.toBeNull();
        expect(manager.getEquippedItem('Weapon')).toBeNull();
    });

    it('getSlots — retorna todos os 6 slots', () => {
        const slots = manager.getSlots();
        const expected: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield'];
        expected.forEach(s => expect(slots.has(s)).toBe(true));
        expect(slots.size).toBe(6);
    });

    it('getEquippedItem — retorna null para slot vazio', () => {
        expect(manager.getEquippedItem('Weapon')).toBeNull();
    });

    it('onEquipmentUpdated — substitui estado completo dos equipamentos', () => {
        const sword = makeWeapon();
        manager.onEquipmentUpdated({ Weapon: sword, Helmet: null, Chest: null, Legs: null, Boots: null, Shield: null });
        expect(manager.getEquippedItem('Weapon')?.id).toBe('w1');
        expect(manager.getEquippedItem('Helmet')).toBeNull();
    });

    it('getTotalAttackBonus — soma bônus das armas equipadas', () => {
        manager.equipItem('Weapon', makeWeapon());
        expect(manager.getTotalAttackBonus()).toBe(5);
    });

    it('getTotalDefenseBonus — soma bônus das armaduras equipadas', () => {
        manager.equipItem('Chest', makeArmor());
        expect(manager.getTotalDefenseBonus()).toBe(3);
    });
});
