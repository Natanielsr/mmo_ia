import type { ItemData, EquipmentSlot } from '../types';

const ALL_SLOTS: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield'];

export class EquipmentUI {
    private panel: HTMLDivElement;
    private slots: Map<EquipmentSlot, HTMLDivElement>;
    private attackSpan: HTMLSpanElement;
    private defenseSpan: HTMLSpanElement;

    public onSlotClick?: (slot: EquipmentSlot) => void;

    constructor() {
        this.panel = document.createElement('div');
        this.panel.id = 'equipment-panel';
        this.panel.style.display = 'none';

        this.slots = new Map();
        for (const slotName of ALL_SLOTS) {
            const el = document.createElement('div');
            el.classList.add('eq-slot');
            el.dataset.slot = slotName;
            el.addEventListener('click', () => this.onSlotClick?.(slotName));
            this.slots.set(slotName, el);
            this.panel.appendChild(el);
        }

        this.attackSpan = document.createElement('span');
        this.attackSpan.id = 'eq-attack';
        this.defenseSpan = document.createElement('span');
        this.defenseSpan.id = 'eq-defense';
        this.panel.appendChild(this.attackSpan);
        this.panel.appendChild(this.defenseSpan);

        document.body.appendChild(this.panel);
    }

    public toggle(): void {
        this.panel.style.display = this.panel.style.display === 'none' ? 'grid' : 'none';
    }

    public updateSlot(slot: EquipmentSlot, item: ItemData): void {
        const el = this.slots.get(slot);
        if (!el) return;
        el.draggable = true;
        el.dataset.itemId = item.id;
        el.dataset.itemType = item.type;
        el.innerHTML = '';
        const name = document.createElement('span');
        name.classList.add('item-name');
        name.textContent = item.name;
        el.appendChild(name);
    }

    public clearSlot(slot: EquipmentSlot): void {
        const el = this.slots.get(slot);
        if (!el) return;
        el.draggable = false;
        delete el.dataset.itemId;
        delete el.dataset.itemType;
        el.innerHTML = '';
    }

    public renderSlots(slotRecord: Record<EquipmentSlot, ItemData | null>): void {
        for (const slot of ALL_SLOTS) {
            const item = slotRecord[slot];
            if (item) this.updateSlot(slot, item);
            else this.clearSlot(slot);
        }
    }

    public updateStats(attackBonus: number, defenseBonus: number): void {
        this.attackSpan.textContent = String(attackBonus);
        this.defenseSpan.textContent = String(defenseBonus);
    }

    public destroy(): void {
        this.panel.remove();
    }

    public getElement(): HTMLDivElement {
        return this.panel;
    }
}
