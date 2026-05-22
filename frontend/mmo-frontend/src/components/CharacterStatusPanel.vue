<template>
  <div class="pointer-events-auto">
    <h3 class="my-3 text-[0.95rem] text-slate-100">
      <span id="char-name">{{ store.playerName || '-' }}</span>
      <span id="level-text" class="text-[0.8em] text-[#ffd700]"> Lv {{ store.level }}</span>
    </h3>

    <div class="relative flex flex-col gap-2">
      <div class="w-full h-3 bg-slate-700 rounded-full overflow-hidden">
        <div id="hp-fill" class="h-full bg-gradient-to-r from-red-500 to-red-400 transition-[width] duration-300" :style="{ width: hpPercent + '%' }" />
      </div>
      <div id="hp-text" class="text-xs font-semibold text-slate-400 text-right">
        {{ Math.ceil(store.hp) }} / {{ store.maxHp }}
      </div>
    </div>

    <div class="relative flex flex-col gap-1 mt-2">
      <div class="w-full h-3 bg-black/50 rounded overflow-hidden">
        <div id="xp-fill" class="h-full transition-[width] duration-300" style="background:#0088ff" :style="{ width: xpPercent + '%' }" />
      </div>
      <div id="xp-text" class="text-[0.8em] text-center mt-[2px] text-slate-400">
        {{ xpIntoLevel }} / {{ xpRequired }} XP
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()

const hpPercent = computed(() => {
  const total = store.maxHp || 1
  return Math.max(0, Math.min(100, (store.hp / total) * 100))
})

const xpIntoLevel = computed(() => {
  const prev = (store.level - 1) * 1000
  return Math.ceil(Math.max(0, store.experience - prev))
})

const xpRequired = computed(() => 1000)

const xpPercent = computed(() =>
  Math.min(100, Math.max(0, (xpIntoLevel.value / xpRequired.value) * 100))
)
</script>
