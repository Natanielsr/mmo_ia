<template>
  <Teleport to="body">
    <div
      id="attributes-modal-overlay"
      v-show="store.attributesModalOpen"
      class="pointer-events-auto"
      style="position:fixed;inset:0;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.6);z-index:1000;"
      @click.self="store.attributesModalOpen = false"
      @keydown.esc="store.attributesModalOpen = false"
    >
      <div class="attributes-modal-content" tabindex="-1">
        <div class="attributes-modal-header">
          <h2>Atributos</h2>
          <button class="attributes-modal-close" @click="store.attributesModalOpen = false">✕</button>
        </div>
        <div class="attributes-modal-body">
          <div class="attr-row attr-row-name">
            <span class="attr-label">Nome:</span>
            <span class="attr-value">{{ store.playerName || '-' }}</span>
          </div>
          <div class="attr-row attr-row-level">
            <span class="attr-label">Level:</span>
            <span class="attr-value">{{ store.level }}</span>
          </div>
          <div class="attr-row attr-row-hp">
            <span class="attr-label">HP:</span>
            <span class="attr-value">{{ Math.ceil(store.hp) }} / {{ store.maxHp }}</span>
          </div>
          <div class="attr-row attr-row-xp">
            <span class="attr-label">XP:</span>
            <span class="attr-value">{{ Math.ceil(store.experience) }}</span>
          </div>
          <div class="attr-row attr-row-attack">
            <span class="attr-label">Ataque:</span>
            <span class="attr-value">{{ store.attackPower ?? '-' }}</span>
          </div>
          <div class="attr-row attr-row-defense">
            <span class="attr-label">Defesa:</span>
            <span class="attr-value">{{ store.defense ?? '-' }}</span>
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
