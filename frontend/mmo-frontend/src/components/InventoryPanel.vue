<template>
  <div
    id="inventory-panel"
    class="pointer-events-auto grid grid-cols-4 gap-2 p-2 rounded-lg bg-slate-800/70 backdrop-blur-md border border-white/10 shadow-[0_8px_32px_rgba(0,0,0,0.6)]"
  >
    <div
      v-for="(item, index) in store.inventorySlots"
      :key="index"
      class="inv-slot"
      :data-index="index"
      :data-item-id="item?.id"
      :data-item-type="item?.type"
      :draggable="item != null"
      @dragstart="item && onDragStart($event, item)"
      @dragend="onDragEnd($event)"
      @dragover.prevent
      @dragenter="onDragEnter($event, index)"
      @dragleave="onDragLeave($event)"
      @drop="onDrop($event, index)"
      @touchstart.passive="item && startTouchDrag($event, item, 'inventory')"
      @touchmove="moveTouchDrag($event)"
      @touchend="endTouchDrag($event)"
      @click.right.prevent="item && store.requestUseItem(item.id)"
    >
      <template v-if="item">
        <img v-if="iconSrc(item.type)" :src="iconSrc(item.type)" class="item-icon-img" />
        <span v-else class="item-icon" :class="iconClass(item.type)" />
        <span class="item-name">{{ item.name }}</span>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'
import { dragState } from '../utils/dragState'
import { useTouchDrag } from '../composables/useTouchDrag'
import type { EquipmentSlot, ItemData } from '../types'

const store = useGameStore()
const { startTouchDrag, moveTouchDrag, endTouchDrag } = useTouchDrag()

const ICON_MAP: Record<string, string> = {
  Armor: 'icon-armor', Helmet: 'icon-helmet',
  Shield: 'icon-shield', Legs: 'icon-legs', Boots: 'icon-boots', Potion: 'icon-potion',
}

const ICON_SRC: Record<string, string> = {
  Weapon: '/assets/items_icon/dagger.png',
}

function iconClass(type: string) {
  return ICON_MAP[type] ?? ''
}

function iconSrc(type: string): string {
  return ICON_SRC[type] ?? ''
}

function onDragStart(e: DragEvent, item: ItemData) {
  dragState.itemId = item.id
  dragState.itemType = item.type
  dragState.source = 'inventory'
  dragState.sourceSlot = ''
  e.dataTransfer!.effectAllowed = 'move'
  e.dataTransfer!.setData('text/plain', item.id)
  ;(e.currentTarget as HTMLElement).classList.add('dragging')
}

function onDragEnd(e: DragEvent) {
  ;(e.currentTarget as HTMLElement).classList.remove('dragging')
  dragState.itemId = ''
  dragState.itemType = ''
  dragState.source = 'inventory'
  dragState.sourceSlot = ''
}

function onDragEnter(e: DragEvent, _index: number) {
  ;(e.currentTarget as HTMLElement).classList.add('drag-over')
}

function onDragLeave(e: DragEvent) {
  const el = e.currentTarget as HTMLElement
  if (!el.contains(e.relatedTarget as Node)) el.classList.remove('drag-over')
}

function onDrop(e: DragEvent, toIndex: number) {
  e.preventDefault()
  const el = e.currentTarget as HTMLElement
  el.classList.remove('drag-over')
  if (!dragState.itemId) return
  if (dragState.source === 'equipment') {
    store.requestUnequip(dragState.sourceSlot as EquipmentSlot)
  } else {
    store.requestMoveInInventory(dragState.itemId, toIndex)
  }
  dragState.itemId = ''
  dragState.itemType = ''
  dragState.source = 'inventory'
  dragState.sourceSlot = ''
}
</script>

<style scoped>
.inv-slot {
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
  padding: 2px;
  gap: 2px;
  touch-action: none;
}

.inv-slot:hover,
.inv-slot.drag-over {
  border-color: #6366f1;
}

.inv-slot.dragging {
  opacity: 0.4;
  border-color: #4f46e5;
}

.item-icon {
  font-size: 1.2rem;
  line-height: 1;
  pointer-events: none;
  user-select: none;
}

.item-name {
  font-size: 0.5rem;
  color: #94a3b8;
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
  padding: 0 2px;
  pointer-events: none;
  user-select: none;
}

.item-icon-img {
  width: 1.4rem;
  height: 1.4rem;
  object-fit: contain;
  pointer-events: none;
  user-select: none;
  image-rendering: pixelated;
}

.icon-armor::before  { content: '🛡️'; }
.icon-potion::before { content: '🧪'; }

@media (max-width: 1024px) {
  .inv-slot { width: 40px; height: 40px; }
  .item-icon { font-size: 1rem; }
  .item-name { font-size: 0.45rem; }
}

@media (max-width: 768px) {
  .inv-slot { width: 36px; height: 36px; padding: 1px; gap: 1px; }
  .item-icon { font-size: 0.9rem; }
  .item-name { font-size: 0.4rem; }
}

@media (max-width: 480px) {
  .inv-slot { width: 32px; height: 32px; padding: 1px; }
  .item-icon { font-size: 0.8rem; }
  .item-name { font-size: 0.35rem; }
}
</style>
