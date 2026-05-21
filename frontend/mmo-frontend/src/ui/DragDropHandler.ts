import type { ItemData, EquipmentSlot } from '../types';
import type { InventoryUI } from './InventoryUI';
import type { EquipmentUI } from './EquipmentUI';

type DropTarget = EquipmentSlot | 'inventory';

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
        //console.log('[DragDrop] canDrop →', { itemId: item.id, itemType: item.type, target });
        if (target === 'inventory') {
            //console.log('[DragDrop] canDrop: inventory — always true');
            return true;
        }
        if (target === 'Weapon') {
            const ok = item.type === 'Weapon';
            //console.log('[DragDrop] canDrop: Weapon slot →', ok);
            return ok;
        }
        if (target === 'Helmet') {
            const ok = item.type === 'Helmet';
            //console.log('[DragDrop] canDrop: Helmet slot →', ok);
            return ok;
        }
        if (target === 'Chest') {
            const ok = item.type === 'Armor';
            //console.log('[DragDrop] canDrop: Chest slot →', ok);
            return ok;
        }
        if (target === 'Legs') {
            const ok = item.type === 'Legs';
            //console.log('[DragDrop] canDrop: Legs slot →', ok);
            return ok;
        }
        if (target === 'Boots') {
            const ok = item.type === 'Boots';
            //console.log('[DragDrop] canDrop: Boots slot →', ok);
            return ok;
        }
        if (target === 'Shield') {
            const ok = item.type === 'Shield';
            //console.log('[DragDrop] canDrop: Shield slot →', ok);
            return ok;
        }
        //console.log('[DragDrop] canDrop: slot desconhecido — false');
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

        let dragItemId    = '';
        let dragItemType  = '';
        let dragSource: 'inventory' | 'equipment' = 'inventory';
        let dragSourceSlot = '';
        let dragSourceEl: HTMLElement | null = null;

        const clearDragState = () => {
            //console.log('[DragDrop] clearDragState — drag encerrado');
            dragSourceEl?.classList.remove('dragging');
            for (const el of document.querySelectorAll<HTMLElement>('.drag-over')) {
                el.classList.remove('drag-over');
            }
            dragItemId = ''; dragItemType = ''; dragSource = 'inventory';
            dragSourceSlot = ''; dragSourceEl = null;
        };

        // ── drag sources ────────────────────────────────────────────────────
        invPanel.addEventListener('dragstart', (e) => {
            const slot = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            if (!slot?.dataset.itemId) { e.preventDefault(); return; }
            dragItemId     = slot.dataset.itemId;
            dragItemType   = slot.dataset.itemType ?? '';
            dragSource     = 'inventory';
            dragSourceSlot = '';
            dragSourceEl   = slot;
            slot.classList.add('dragging');
            e.dataTransfer!.effectAllowed = 'move';
            e.dataTransfer!.setData('text/plain', dragItemId);
            //console.log('[DragDrop] dragstart inventory →', { dragItemId, dragItemType });
        });

        eqPanel.addEventListener('dragstart', (e) => {
            const slot = (e.target as HTMLElement).closest<HTMLElement>('.eq-slot');
            if (!slot?.dataset.itemId) { e.preventDefault(); return; }
            dragItemId     = slot.dataset.itemId;
            dragItemType   = slot.dataset.itemType ?? '';
            dragSource     = 'equipment';
            dragSourceSlot = slot.dataset.slot ?? '';
            dragSourceEl   = slot;
            e.dataTransfer!.effectAllowed = 'move';
            e.dataTransfer!.setData('text/plain', dragItemId);
            //console.log('[DragDrop] dragstart equipment →', { dragItemId, dragItemType, dragSourceSlot });
        });

        invPanel.addEventListener('dragend', clearDragState);
        eqPanel.addEventListener('dragend', clearDragState);

        // ── inventory drop target ────────────────────────────────────────────
        invPanel.addEventListener('dragover', (e) => {
            if ((e.target as HTMLElement).closest('.inv-slot')) e.preventDefault();
        });

        invPanel.addEventListener('dragenter', (e) => {
            const target = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            if (!target || !dragItemId) return;
            target.classList.add('drag-over');
        });

        invPanel.addEventListener('dragleave', (e) => {
            const target = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            if (!target || target.contains(e.relatedTarget as Node)) return;
            target.classList.remove('drag-over');
        });

        invPanel.addEventListener('drop', (e) => {
            e.preventDefault();
            const target = (e.target as HTMLElement).closest<HTMLElement>('.inv-slot');
            target?.classList.remove('drag-over');
            if (!target || !dragItemId) return;
            const toIndex = parseInt(target.dataset.index ?? '0', 10);
            //console.log('[DragDrop] drop inventory → target index:', toIndex, '| source:', dragSource, '| dragSourceSlot:', dragSourceSlot);
            if (dragSource === 'inventory') {
                this.onMoveInInventory(dragItemId, toIndex);
            } else {
                this.onUnequip(dragSourceSlot as EquipmentSlot);
            }
            dragItemId = '';
        });

        // ── equipment drop target (per-slot to avoid empty-cell miss) ────────
        for (const slotEl of eqPanel.querySelectorAll<HTMLElement>('.eq-slot')) {
            const fakeItem = (): ItemData => ({ id: dragItemId, name: '', position: { x: 0, y: 0 }, type: dragItemType });

            slotEl.addEventListener('dragenter', (e) => {
                e.preventDefault();
                if (!dragItemId) return;
                const canDrop = this.canDrop(fakeItem(), slotEl.dataset.slot as EquipmentSlot);
                //console.log('[DragDrop] dragenter eq-slot:', slotEl.dataset.slot, '| canDrop:', canDrop);
                if (canDrop) {
                    slotEl.classList.add('drag-over');
                }
            });

            slotEl.addEventListener('dragleave', (e) => {
                if (slotEl.contains(e.relatedTarget as Node)) return;
                slotEl.classList.remove('drag-over');
            });

            slotEl.addEventListener('dragover', (e) => {
                if (!dragItemId) return;
                const canDrop = this.canDrop(fakeItem(), slotEl.dataset.slot as EquipmentSlot);
                //console.log('[DragDrop] dragover eq-slot:', slotEl.dataset.slot, '| canDrop:', canDrop);
                if (canDrop) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });

            slotEl.addEventListener('drop', (e) => {
                e.preventDefault();
                e.stopPropagation();
                slotEl.classList.remove('drag-over');
                if (!dragItemId) return;
                const targetSlot = slotEl.dataset.slot as EquipmentSlot;
                const canDrop = this.canDrop(fakeItem(), targetSlot);
                //console.log('[DragDrop] drop eq-slot:', targetSlot, '| dragItemId:', dragItemId, '| canDrop:', canDrop);
                if (canDrop) {
                    //console.log('[DragDrop] ✅ onEquip chamado com itemId:', dragItemId);
                    this.onEquip(dragItemId);
                } else {
                    //console.log('[DragDrop] ❌ canDrop retornou false — onEquip NÃO chamado');
                }
                dragItemId = '';
            });
        }
    }
}
