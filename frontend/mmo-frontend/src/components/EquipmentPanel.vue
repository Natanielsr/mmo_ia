<template>
  <div id="equipment-panel" class="pointer-events-auto">
    <div
      v-for="slotName in ALL_SLOTS"
      :key="slotName"
      class="eq-slot"
      :data-slot="slotName"
      :data-item-id="store.equipmentSlots[slotName]?.id"
      :data-item-type="store.equipmentSlots[slotName]?.type"
      :draggable="store.equipmentSlots[slotName] != null"
      @dragstart="store.equipmentSlots[slotName] && onDragStart($event, slotName)"
      @dragover="onDragOver($event, slotName)"
      @dragleave="onDragLeave($event)"
      @drop="onDrop($event, slotName)"
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
import type { EquipmentSlot, ItemData } from '../types'

const store = useGameStore()

const ALL_SLOTS: EquipmentSlot[] = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield']

let dragItemId = ''
let dragItemType = ''

function onDragStart(e: DragEvent, slotName: EquipmentSlot) {
  const item = store.equipmentSlots[slotName]
  if (!item) return
  dragItemId = item.id
  dragItemType = item.type
  e.dataTransfer!.effectAllowed = 'move'
  e.dataTransfer!.setData('text/plain', item.id)
}

function onDragOver(e: DragEvent, slotName: EquipmentSlot) {
  if (!dragItemId) return
  const fakeItem: ItemData = { id: dragItemId, name: '', position: { x: 0, y: 0 }, type: dragItemType }
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
  if (!dragItemId) return
  const fakeItem: ItemData = { id: dragItemId, name: '', position: { x: 0, y: 0 }, type: dragItemType }
  if (canDropOnSlot(fakeItem, slotName)) {
    store.requestEquip(dragItemId)
  }
  dragItemId = ''
  dragItemType = ''
}
</script>
