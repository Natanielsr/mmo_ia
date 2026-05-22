<template>
  <div
    v-show="store.gearPanelOpen"
    id="gear-panel"
    class="pointer-events-auto sidebar glass"
    style="position:fixed;top:0;right:0;height:100%;z-index:150;display:flex;flex-direction:column;gap:8px;padding:8px;"
  >
    <CharacterStatusPanel />
    <EquipmentPanel />
    <InventoryPanel />
    <div class="coordinates" style="font-size:0.8em;color:#94a3b8;margin-top:auto;">
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
