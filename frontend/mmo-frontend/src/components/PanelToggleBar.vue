<template>
  <div
    id="panel-toggle-bar"
    v-show="store.panelBarVisible"
    class="pointer-events-auto"
  >
    <button
      id="gear-toggle"
      class="panel-toggle-btn"
      :class="{ active: store.gearPanelOpen }"
      title="Inventário & Equipamento"
      @click="store.toggleGearPanel()"
    >
      <span class="toggle-icon">🎒</span>
    </button>
    <button
      id="attributes-toggle"
      class="panel-toggle-btn"
      :class="{ active: store.attributesModalOpen }"
      title="Atributos"
      @click="store.toggleAttributesModal()"
    >
      <span class="toggle-icon">📋</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()
</script>
