import type { ItemData } from '../types';

export class InventoryUI {
    private panel: HTMLDivElement;
    private slots: HTMLDivElement[];
    private slotItems: Map<number, ItemData> = new Map();

    public onSlotClick?: (index: number) => void;
    public onSlotRightClick?: (itemId: string) => void;

    constructor() {
        this.panel = document.createElement('div');
        this.panel.id = 'inventory-panel';
        this.panel.style.display = 'none';

        this.slots = [];
        for (let i = 0; i < 20; i++) {
            const slot = document.createElement('div');
            slot.classList.add('inv-slot');
            slot.dataset.index = String(i);
            slot.addEventListener('click', () => this.onSlotClick?.(i));
            slot.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                const item = this.slotItems.get(i);
                if (item) this.onSlotRightClick?.(item.id);
            });
            this.slots.push(slot);
            this.panel.appendChild(slot);
        }

        document.body.appendChild(this.panel);
    }

    public toggle(): void {
        this.panel.style.display = this.panel.style.display === 'none' ? 'grid' : 'none';
    }

    public updateSlot(index: number, item: ItemData): void {
        const slot = this.slots[index];
        if (!slot) return;
        this.slotItems.set(index, item);
        slot.draggable = true;
        slot.dataset.itemId = item.id;
        slot.dataset.itemType = item.type;
        slot.innerHTML = '';

        const icon = document.createElement('span');
        icon.classList.add('item-icon');
        if (item.type === 'Weapon') icon.classList.add('icon-sword');
        else if (item.type === 'Armor') icon.classList.add('icon-armor');
        else if (item.type === 'Potion') icon.classList.add('icon-potion');

        const name = document.createElement('span');
        name.classList.add('item-name');
        name.textContent = item.name;

        slot.appendChild(icon);
        slot.appendChild(name);
    }

    public clearSlot(index: number): void {
        const slot = this.slots[index];
        if (!slot) return;
        this.slotItems.delete(index);
        slot.draggable = false;
        delete slot.dataset.itemId;
        delete slot.dataset.itemType;
        slot.innerHTML = '';
    }

    public renderItems(items: ItemData[]): void {
        for (let i = 0; i < 20; i++) this.clearSlot(i);
        items.forEach((item, listIdx) => this.updateSlot(item.slotIndex ?? listIdx, item));
    }

    public destroy(): void {
        this.panel.remove();
    }
}
