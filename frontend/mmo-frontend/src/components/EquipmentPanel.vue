<template>
  <div
    id="equipment-panel"
    class="pointer-events-auto grid gap-2 p-2 rounded-lg bg-slate-800/70 backdrop-blur-md border border-white/10 shadow-[0_8px_32px_rgba(0,0,0,0.6)]"
  >
    <div
      v-for="slotName in ALL_SLOTS"
      :key="slotName"
      class="eq-slot"
      :data-slot="slotName"
      :data-item-id="store.equipmentSlots[slotName]?.id"
      :data-item-type="store.equipmentSlots[slotName]?.type"
      :draggable="store.equipmentSlots[slotName] != null"
      @dragstart="store.equipmentSlots[slotName] && onDragStart($event, slotName)"
      @dragend="onDragEnd($event)"
      @dragover="onDragOver($event, slotName)"
      @dragleave="onDragLeave($event)"
      @drop="onDrop($event, slotName)"
      @touchstart.passive="store.equipmentSlots[slotName] && startTouchDrag($event, store.equipmentSlots[slotName]!, 'equipment', slotName)"
      @touchmove="moveTouchDrag($event)"
      @touchend="endTouchDrag($event)"
      @click="store.equipmentSlots[slotName] && store.requestUnequip(slotName)"
    >
      <template v-if="store.equipmentSlots[slotName]">
        <span class="item-name">{{ store.equipmentSlots[slotName]!.name }}</span>
      </template>
    </div>

    <div class="eq-stats">
      <span id="eq-attack">{{ store.equipAttackBonus }}</span>
      <span id="eq-defense">{{ store.equipDefenseBonus }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'
import { canDropOnSlot } from '../utils/dragDropRules'
import { dragState } from '../utils/dragState'
import { useTouchDrag } from '../composables/useTouchDrag'
import type { EquipmentSlot, ItemData } from '../types'

const store = useGameStore()
const { startTouchDrag, moveTouchDrag, endTouchDrag } = useTouchDrag()

const ALL_SLOTS: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield']

function onDragStart(e: DragEvent, slotName: EquipmentSlot) {
  const item = store.equipmentSlots[slotName]
  if (!item) return
  dragState.itemId = item.id
  dragState.itemType = item.type
  dragState.source = 'equipment'
  dragState.sourceSlot = slotName
  e.dataTransfer!.effectAllowed = 'move'
  e.dataTransfer!.setData('text/plain', item.id)
}

function onDragEnd(_e: DragEvent) {
  dragState.itemId = ''
  dragState.itemType = ''
  dragState.source = 'inventory'
  dragState.sourceSlot = ''
}

function onDragOver(e: DragEvent, slotName: EquipmentSlot) {
  if (!dragState.itemId) return
  const fakeItem: ItemData = { id: dragState.itemId, name: '', position: { x: 0, y: 0 }, type: dragState.itemType }
  if (canDropOnSlot(fakeItem, slotName)) {
    e.preventDefault()
    ;(e.currentTarget as HTMLElement).classList.add('drag-over')
  }
}

function onDragLeave(e: DragEvent) {
  const el = e.currentTarget as HTMLElement
  if (!el.contains(e.relatedTarget as Node)) el.classList.remove('drag-over')
}

function onDrop(e: DragEvent, slotName: EquipmentSlot) {
  e.preventDefault()
  const el = e.currentTarget as HTMLElement
  el.classList.remove('drag-over')
  if (!dragState.itemId) return
  const fakeItem: ItemData = { id: dragState.itemId, name: '', position: { x: 0, y: 0 }, type: dragState.itemType }
  if (canDropOnSlot(fakeItem, slotName)) {
    store.requestEquip(dragState.itemId)
  }
  dragState.itemId = ''
  dragState.itemType = ''
}
</script>

<style scoped>
#equipment-panel {
  grid-template-columns: repeat(3, 44px);
  grid-template-rows: repeat(4, 44px);
}

.eq-slot {
  width: 44px;
  height: 44px;
  background: rgba(0, 0, 0, 0.4);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: border-color 0.15s;
  overflow: hidden;
  padding: 3px;
  gap: 2px;
  position: relative;
  touch-action: none;
}

.eq-slot:hover,
.eq-slot.drag-over {
  border-color: #6366f1;
}

.eq-slot::after {
  content: attr(data-slot);
  font-size: 0.45rem;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  position: absolute;
  bottom: 2px;
}

.item-name {
  font-size: 0.5rem;
  color: #f8fafc;
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
  padding: 0 2px;
  margin-bottom: 8px;
  pointer-events: none;
  user-select: none;
}

/* Cross layout: col2=center, weapon/chest/shield=row2, legs/boots=rows3-4 */
.eq-slot[data-slot="Helmet"] { grid-column: 2; grid-row: 1; }
.eq-slot[data-slot="Weapon"] { grid-column: 1; grid-row: 2; }
.eq-slot[data-slot="Chest"]  { grid-column: 2; grid-row: 2; }
.eq-slot[data-slot="Shield"] { grid-column: 3; grid-row: 2; }
.eq-slot[data-slot="Legs"]   { grid-column: 2; grid-row: 3; }
.eq-slot[data-slot="Boots"]  { grid-column: 2; grid-row: 4; }

.eq-stats { display: none; }

@media (max-width: 1024px) {
  #equipment-panel {
    grid-template-columns: repeat(3, 40px);
    grid-template-rows: repeat(4, 40px);
  }
  .eq-slot { width: 40px; height: 40px; }
  .item-name { font-size: 0.45rem; margin-bottom: 6px; }
  .eq-slot::after { font-size: 0.4rem; }
}

@media (max-width: 768px) {
  #equipment-panel {
    grid-template-columns: repeat(3, 36px);
    grid-template-rows: repeat(4, 36px);
  }
  .eq-slot { width: 36px; height: 36px; padding: 2px; }
  .item-name { font-size: 0.4rem; margin-bottom: 5px; }
  .eq-slot::after { font-size: 0.35rem; bottom: 1px; }
}

@media (max-width: 480px) {
  #equipment-panel {
    grid-template-columns: repeat(3, 32px);
    grid-template-rows: repeat(4, 32px);
  }
  .eq-slot { width: 32px; height: 32px; padding: 1px; }
  .item-name { font-size: 0.35rem; margin-bottom: 4px; }
  .eq-slot::after { font-size: 0.3rem; bottom: 0; }
}
</style>
