<template>
  <div
    v-show="store.gearPanelOpen"
    id="gear-panel"
    class="fixed top-0 right-0 h-full z-[150] flex flex-col gap-2 p-2 bg-slate-800/70 backdrop-blur-md border-l border-white/10 rounded-l-xl pointer-events-auto"
  >
    <CharacterStatusPanel />
    <EquipmentPanel />
    <InventoryPanel />
    <div class="mt-auto text-[0.8em] text-slate-400 pt-2 border-t border-white/10">
      Posição: <span>{{ store.posX }}</span>, <span>{{ store.posY }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'
import CharacterStatusPanel from './CharacterStatusPanel.vue'
import EquipmentPanel from './EquipmentPanel.vue'
import InventoryPanel from './InventoryPanel.vue'

const store = useGameStore()
</script>
