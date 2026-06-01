<template>
  <Teleport to="body">
    <div
      v-show="store.rankingPanelOpen && store.isLoggedIn"
      class="ranking-panel fixed top-4 left-4 z-[100] w-[340px] rounded-xl bg-slate-800/90 backdrop-blur-md border border-indigo-500/30 shadow-[0_8px_32px_rgba(0,0,0,0.7)]"
    >
      <div class="flex justify-between items-center px-4 py-3 border-b border-white/10 bg-indigo-500/10">
        <h2 class="text-sm font-bold text-indigo-300 uppercase tracking-widest m-0">Hall da Fama</h2>
        <button
          class="text-slate-400 hover:text-white text-lg leading-none cursor-pointer bg-transparent border-none"
          @click="store.rankingPanelOpen = false"
        >✕</button>
      </div>

      <div class="px-3 py-2">
        <div v-if="store.ranking.length === 0" class="text-center text-slate-500 text-xs py-4">
          Nenhum herói caiu ainda.
        </div>
        <table v-else class="w-full text-xs">
          <thead>
            <tr class="text-slate-400 border-b border-white/10">
              <th class="text-left py-1 pr-2 w-6">#</th>
              <th class="text-left py-1 pr-2">Nome</th>
              <th class="text-right py-1 pr-2">Lv</th>
              <th class="text-right py-1 pr-2">XP</th>
              <th class="text-right py-1">Tempo</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(entry, i) in store.ranking"
              :key="i"
              class="border-b border-white/5 hover:bg-white/5 transition-colors"
            >
              <td class="py-1 pr-2 text-slate-500">{{ i + 1 }}</td>
              <td class="py-1 pr-2 text-indigo-300 font-medium truncate max-w-[100px]">{{ entry.name }}</td>
              <td class="py-1 pr-2 text-amber-400 text-right font-bold">{{ entry.level }}</td>
              <td class="py-1 pr-2 text-green-400 text-right">{{ formatXp(entry.totalExperience) }}</td>
              <td class="py-1 text-slate-400 text-right">{{ formatTime(entry.timeSurvivedSeconds) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="px-3 py-2 border-t border-white/10 text-[10px] text-slate-600 text-center">
        Tab — abre/fecha
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()

function formatTime(seconds: number): string {
  if (seconds < 60) return `${seconds}s`
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return s > 0 ? `${m}m ${s}s` : `${m}m`
}

function formatXp(xp: number): string {
  if (xp >= 1000) return `${(xp / 1000).toFixed(1)}k`
  return String(xp)
}
</script>
