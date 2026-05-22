<template>
  <div id="inventory-panel" class="pointer-events-auto">
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
