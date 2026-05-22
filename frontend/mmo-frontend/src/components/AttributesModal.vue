<template>
  <Teleport to="body">
    <div
      id="attributes-modal-overlay"
      v-show="store.attributesModalOpen"
      class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/60 backdrop-blur-sm pointer-events-auto"
      @click.self="store.attributesModalOpen = false"
      @keydown.esc="store.attributesModalOpen = false"
    >
      <div
        class="w-[90%] max-w-[400px] max-h-[80vh] overflow-y-auto rounded-xl bg-slate-800/70 backdrop-blur-md border border-white/10 shadow-[0_12px_48px_rgba(0,0,0,0.8)]"
        tabindex="-1"
      >
        <div class="flex justify-between items-center px-6 py-4 border-b border-white/10 bg-indigo-500/10">
          <h2 class="text-2xl text-indigo-400 font-semibold m-0">Atributos</h2>
          <button
            class="attributes-modal-close bg-transparent border-none text-slate-400 text-2xl cursor-pointer p-0 w-8 h-8 flex items-center justify-center rounded hover:bg-white/5 hover:text-slate-100 transition-all duration-200"
            @click="store.attributesModalOpen = false"
          >
            ✕
          </button>
        </div>
        <div class="px-6 py-8 flex flex-col gap-4">
          <div class="attr-row-name flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Nome:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ store.playerName || '-' }}</span>
          </div>
          <div class="attr-row-level flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Level:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ store.level }}</span>
          </div>
          <div class="attr-row-hp flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">HP:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ Math.ceil(store.hp) }} / {{ store.maxHp }}</span>
          </div>
          <div class="attr-row-xp flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">XP:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ Math.ceil(store.experience) }}</span>
          </div>
          <div class="attr-row-attack flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Ataque:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ store.attackPower ?? '-' }}</span>
          </div>
          <div class="attr-row-defense flex justify-between items-center p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Defesa:</span>
            <span class="attr-value text-indigo-400 font-bold text-[1.1rem]">{{ store.defense ?? '-' }}</span>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()

function onKeyDown(e: KeyboardEvent) {
  if (e.key === 'Escape' && store.attributesModalOpen) {
    store.attributesModalOpen = false
  }
}

onMounted(() => document.addEventListener('keydown', onKeyDown))
onUnmounted(() => document.removeEventListener('keydown', onKeyDown))
</script>
