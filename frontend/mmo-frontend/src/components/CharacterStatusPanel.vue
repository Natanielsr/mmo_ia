<template>
  <div class="pointer-events-auto">
    <h3 class="char-title">
      <span id="char-name">{{ store.playerName || '-' }}</span>
      <span id="level-text" style="font-size:0.8em;color:#ffd700;"> Lv {{ store.level }}</span>
    </h3>

    <div class="hp-container">
      <div class="hp-bar progress-bar">
        <div id="hp-fill" :style="{ width: hpPercent + '%' }" />
      </div>
      <div id="hp-text">{{ Math.ceil(store.hp) }} / {{ store.maxHp }}</div>
    </div>

    <div class="xp-container" style="margin-top:5px;">
      <div class="xp-bar progress-bar" style="background:rgba(0,0,0,0.5);border-radius:4px;overflow:hidden;height:12px;position:relative;">
        <div id="xp-fill" :style="{ width: xpPercent + '%', height: '100%', background: '#0088ff', transition: 'width 0.3s ease' }" />
      </div>
      <div id="xp-text" style="font-size:0.8em;text-align:center;margin-top:2px;">
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
