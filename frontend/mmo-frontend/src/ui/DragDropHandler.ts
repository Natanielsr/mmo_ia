import type { ItemData, EquipmentSlot } from '../types';
import type { InventoryUI } from './InventoryUI';
import type { EquipmentUI } from './EquipmentUI';

type DropTarget = EquipmentSlot | 'inventory';

const ARMOR_SLOTS: DropTarget[] = ['Helmet', 'Chest', 'Legs', 'Boots', 'Shield'];

export class DragDropHandler {
    private readonly onEquip: (itemId: string) => void;
    private readonly onUnequip: (slot: EquipmentSlot) => void;
    private readonly onMoveInInventory: (itemId: string, toIndex: number) => void;

    constructor(
        onEquip: (itemId: string) => void,
        onUnequip: (slot: EquipmentSlot) => void,
        onMoveInInventory: (itemId: string, toIndex: number) => void,
    ) {
        this.onEquip = onEquip;
        this.onUnequip = onUnequip;
        this.onMoveInInventory = onMoveInInventory;
    }

    public canDrop(item: ItemData, target: DropTarget): boolean {
        if (target === 'inventory') return true;
        if (target === 'Weapon') return item.type === 'Weapon';
        if (ARMOR_SLOTS.includes(target)) return item.type === 'Armor';
        return false;
    }

    public handleDrop(item: ItemData, target: 'inventory', toIndex: number): void;
    public handleDrop(item: ItemData, target: EquipmentSlot): void;
    public handleDrop(item: ItemData, target: DropTarget, toIndex?: number): void {
        if (target === 'inventory') {
            this.onMoveInInventory(item.id, toIndex ?? 0);
        } else {
            this.onEquip(item.id);
        }
    }

    public setup(_invUI: InventoryUI, _eqUI: EquipmentUI): void {
        const invPanel = document.getElementById('inventory-panel');
        const eqPanel  = document.getElementById('equipment-panel');
        if (!invPanel || !eqPanel) return;

        let dragItemId   = '';
        let dragItemType = '';
        let dragSource: 'inventory' | 'equipment' = 'inventory';
        let dragSourceSlot = '';

        // ── drag sources ────────────────────────────────────────────────────
        invPanel.addEventListener('dragstart', (e) => {
            const slot = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            if (!slot?.dataset.itemId) { e.preventDefault(); return; }
            dragItemId   = slot.dataset.itemId;
            dragItemType = slot.dataset.itemType ?? '';
            dragSource   = 'inventory';
            dragSourceSlot = '';
            e.dataTransfer!.effectAllowed = 'move';
            e.dataTransfer!.setData('text/plain', dragItemId);
        });

        eqPanel.addEventListener('dragstart', (e) => {
            const slot = (e.target as HTMLElement).closest<HTMLElement>('.eq-slot');
            if (!slot?.dataset.itemId) { e.preventDefault(); return; }
            dragItemId     = slot.dataset.itemId;
            dragItemType   = slot.dataset.itemType ?? '';
            dragSource     = 'equipment';
            dragSourceSlot = slot.dataset.slot ?? '';
            e.dataTransfer!.effectAllowed = 'move';
            e.dataTransfer!.setData('text/plain', dragItemId);
        });

        // ── inventory drop target ────────────────────────────────────────────
        invPanel.addEventListener('dragover', (e) => {
            if ((e.target as HTMLElement).closest('.inv-slot')) e.preventDefault();
        });

        invPanel.addEventListener('drop', (e) => {
            e.preventDefault();
            const target = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            if (!target || !dragItemId) return;
            const toIndex = parseInt(target.dataset.index ?? '0', 10);
            if (dragSource === 'inventory') {
                this.onMoveInInventory(dragItemId, toIndex);
            } else {
                this.onUnequip(dragSourceSlot as EquipmentSlot);
            }
            dragItemId = '';
        });

        // ── equipment drop target (per-slot to avoid empty-cell miss) ────────
        for (const slotEl of eqPanel.querySelectorAll<HTMLElement>('.eq-slot')) {
            slotEl.addEventListener('dragover', (e) => {
                if (!dragItemId) return;
                const fakeItem: ItemData = { id: dragItemId, name: '', position: { x: 0, y: 0 }, type: dragItemType };
                if (this.canDrop(fakeItem, slotEl.dataset.slot as EquipmentSlot)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
            slotEl.addEventListener('drop', (e) => {
                e.preventDefault();
                e.stopPropagation();
                if (!dragItemId) return;
                const targetSlot = slotEl.dataset.slot as EquipmentSlot;
                const fakeItem: ItemData = { id: dragItemId, name: '', position: { x: 0, y: 0 }, type: dragItemType };
                if (this.canDrop(fakeItem, targetSlot)) this.onEquip(dragItemId);
                dragItemId = '';
            });
        }
    }
}
