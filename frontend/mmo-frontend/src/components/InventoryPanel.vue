<template>
  <div
    id="inventory-panel"
    class="pointer-events-auto grid grid-cols-4 gap-1 p-[5px] rounded-lg bg-slate-800/70 backdrop-blur-md border border-white/10 shadow-[0_8px_32px_rgba(0,0,0,0.6)]"
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
      @dragover.prevent
      @dragenter="onDragEnter($event, index)"
      @dragleave="onDragLeave($event)"
      @drop="onDrop($event, index)"
      @click.right.prevent="item && store.requestUseItem(item.id)"
    >
      <template v-if="item">
        <span class="item-icon" :class="iconClass(item.type)" />
        <span class="item-name">{{ item.name }}</span>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'
import type { ItemData } from '../types'

const store = useGameStore()

const ICON_MAP: Record<string, string> = {
  Weapon: 'icon-sword', Armor: 'icon-armor', Helmet: 'icon-helmet',
  Shield: 'icon-shield', Legs: 'icon-legs', Boots: 'icon-boots', Potion: 'icon-potion',
}

function iconClass(type: string) {
  return ICON_MAP[type] ?? ''
}

let dragItemId = ''
let dragItemType = ''

function onDragStart(e: DragEvent, item: ItemData) {
  dragItemId = item.id
  dragItemType = item.type
  e.dataTransfer!.effectAllowed = 'move'
  e.dataTransfer!.setData('text/plain', item.id)
  ;(e.currentTarget as HTMLElement).classList.add('dragging')
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
  if (!dragItemId) return
  store.requestMoveInInventory(dragItemId, toIndex)
  dragItemId = ''
  dragItemType = ''
}
</script>

<style scoped>
.inv-slot {
  width: 54px;
  height: 54px;
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

.icon-sword::before  { content: '⚔️'; }
.icon-armor::before  { content: '🛡️'; }
.icon-potion::before { content: '🧪'; }

@media (max-width: 1024px) {
  .inv-slot { width: 48px; height: 48px; }
  .item-icon { font-size: 1rem; }
  .item-name { font-size: 0.45rem; }
}

@media (max-width: 768px) {
  .inv-slot { width: 40px; height: 40px; padding: 1px; gap: 1px; }
  .item-icon { font-size: 0.9rem; }
  .item-name { font-size: 0.4rem; }
}

@media (max-width: 480px) {
  .inv-slot { width: 36px; height: 36px; padding: 1px; }
  .item-icon { font-size: 0.8rem; }
  .item-name { font-size: 0.35rem; }
}
</style>
