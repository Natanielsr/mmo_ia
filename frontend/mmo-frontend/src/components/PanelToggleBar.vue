<template>
  <div
    id="panel-toggle-bar"
    v-show="store.panelBarVisible"
    class="fixed bottom-[10px] left-1/2 -translate-x-1/2 z-[700] flex flex-row gap-[6px] bg-slate-900/85 border border-indigo-500/30 rounded-xl p-2 pointer-events-auto"
  >
    <button
      id="gear-toggle"
      :class="[
        'panel-toggle-btn w-11 h-11 border-2 border-indigo-500/40 rounded-lg text-white text-[1.2rem] flex items-center justify-center cursor-pointer transition-all duration-200 backdrop-blur-md p-0 max-[768px]:w-12 max-[768px]:h-12 max-[768px]:text-[1.3rem]',
        store.gearPanelOpen ? 'active bg-indigo-500/50 border-indigo-500/90 shadow-[0_0_16px_rgba(99,102,241,0.5)]' : 'bg-slate-800/90 hover:bg-slate-800 hover:border-indigo-500/70 hover:shadow-[0_0_12px_rgba(99,102,241,0.3)]'
      ]"
      title="Inventário & Equipamento"
      @click="store.toggleGearPanel()"
    >
      <span class="flex items-center justify-center w-full h-full">🎒</span>
    </button>
    <button
      id="attributes-toggle"
      :class="[
        'panel-toggle-btn w-11 h-11 border-2 border-indigo-500/40 rounded-lg text-white text-[1.2rem] flex items-center justify-center cursor-pointer transition-all duration-200 backdrop-blur-md p-0 max-[768px]:w-12 max-[768px]:h-12 max-[768px]:text-[1.3rem]',
        store.attributesModalOpen ? 'active bg-indigo-500/50 border-indigo-500/90 shadow-[0_0_16px_rgba(99,102,241,0.5)]' : 'bg-slate-800/90 hover:bg-slate-800 hover:border-indigo-500/70 hover:shadow-[0_0_12px_rgba(99,102,241,0.3)]'
      ]"
      title="Atributos"
      @click="store.toggleAttributesModal()"
    >
      <span class="flex items-center justify-center w-full h-full">📋</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()
</script>
