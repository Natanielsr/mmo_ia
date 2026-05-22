<template>
  <div class="pointer-events-auto fixed bottom-0 left-0 right-0 max-h-36 overflow-y-auto bg-slate-900/80 backdrop-blur border-t border-white/10 px-3 py-2 z-10">
    <h3 class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Log de Atividades</h3>
    <div id="log-content">
      <div
        v-for="entry in store.logEntries"
        :key="entry.id"
        class="log-entry text-xs py-0.5"
        :class="entry.isError ? 'error text-red-400' : 'text-slate-300'"
      >{{ entry.message }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()
</script>
