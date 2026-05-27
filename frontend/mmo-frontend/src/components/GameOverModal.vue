<template>
  <Teleport to="body">
    <div
      v-show="store.isDead && store.isLoggedIn"
      class="game-over-overlay fixed inset-0 z-[2000] flex items-center justify-center bg-black/80 backdrop-blur-sm pointer-events-auto"
    >
      <div class="w-[90%] max-w-[360px] rounded-xl bg-slate-800/70 backdrop-blur-md border border-red-500/30 shadow-[0_12px_48px_rgba(0,0,0,0.8)]">
        <div class="flex justify-center items-center px-6 py-4 border-b border-white/10 bg-red-500/10">
          <h2 class="text-2xl text-red-400 font-semibold m-0">GAME OVER</h2>
        </div>
        <div class="px-6 py-8 flex flex-col gap-4 items-center text-center">
          <div class="flex justify-between items-center w-full p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Personagem:</span>
            <span class="text-indigo-400 font-bold text-[1.1rem]">{{ store.playerName }}</span>
          </div>
          <div class="flex justify-between items-center w-full p-3 bg-black/20 rounded-md border border-white/10">
            <span class="text-slate-400 font-medium text-[0.95rem]">Level:</span>
            <span class="text-indigo-400 font-bold text-[1.1rem]">{{ store.level }}</span>
          </div>
          <p class="text-slate-500 text-xs mt-2">Todo o progresso desta sessão foi perdido.</p>
          <button
            class="mt-2 w-full bg-indigo-500 hover:bg-indigo-600 text-white font-semibold px-6 py-2 rounded-lg transition-all duration-200 cursor-pointer border-none"
            @click="restart"
          >
            Reiniciar
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()

function restart() {
  window.location.reload()
}
</script>

<style scoped>
.game-over-overlay {
  animation: fadeIn 3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to   { opacity: 1; }
}
</style>
