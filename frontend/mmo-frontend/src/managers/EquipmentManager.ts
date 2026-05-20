import type { ItemData, EquipmentSlot } from '../types';

const ALL_SLOTS: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield'];

export class EquipmentManager {
    private slots: Map<EquipmentSlot, ItemData | null>;

    constructor() {
        this.slots = new Map(ALL_SLOTS.map(s => [s, null]));
    }

    public equipItem(slot: EquipmentSlot, item: ItemData): ItemData | null {
        const displaced = this.slots.get(slot) ?? null;
        this.slots.set(slot, item);
        return displaced;
    }

    public unequipItem(slot: EquipmentSlot): ItemData | null {
        const item = this.slots.get(slot) ?? null;
        this.slots.set(slot, null);
        return item;
    }

    public getEquippedItem(slot: EquipmentSlot): ItemData | null {
        return this.slots.get(slot) ?? null;
    }

    public getSlots(): Map<EquipmentSlot, ItemData | null> {
        return new Map(this.slots);
    }

    public getTotalAttackBonus(): number {
        let total = 0;
        for (const item of this.slots.values()) {
            if (item?.type === 'Weapon') total += item.attackBonus ?? 0;
        }
        return total;
    }

    public getTotalDefenseBonus(): number {
        let total = 0;
        for (const item of this.slots.values()) {
            if (item?.type === 'Armor') total += item.defenseBonus ?? 0;
        }
        return total;
    }

    public onEquipmentUpdated(slotRecord: Record<EquipmentSlot, ItemData | null>): void {
        for (const slot of ALL_SLOTS) {
            this.slots.set(slot, slotRecord[slot] ?? null);
        }
    }
}
