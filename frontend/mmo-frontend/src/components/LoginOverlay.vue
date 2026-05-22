<template>
  <div
    id="login-overlay"
    v-show="!store.isLoggedIn"
    class="fixed inset-0 z-[100] flex items-center justify-center bg-black/80 pointer-events-auto"
  >
    <div class="w-[400px] text-center p-10 rounded-xl bg-slate-800/70 backdrop-blur-md border border-white/10 shadow-[0_25px_50px_-12px_rgba(0,0,0,0.5)] max-[768px]:w-[90%] max-[768px]:p-6">
      <h1 class="text-4xl font-extrabold mb-2 bg-gradient-to-r from-indigo-400 to-purple-400 bg-clip-text text-transparent">
        MMORPG Web
      </h1>
      <p class="text-slate-400 mb-8">Entre no mundo e começe sua aventura</p>

      <div
        id="connection-error"
        v-show="store.connectionError"
        class="bg-red-500/20 border border-red-500 text-red-300 p-3 rounded-lg mb-6 text-sm leading-relaxed text-center"
      >
        Não foi possível conectar ao servidor.
      </div>

      <input
        id="username"
        v-model="playerName"
        type="text"
        placeholder="Nome da personagem..."
        maxlength="12"
        class="w-full bg-black/30 border border-white/10 px-4 py-3 rounded-lg text-white mb-6 outline-none focus:border-indigo-500 placeholder:text-slate-400"
        @keydown.stop
        @keyup.stop
        @keypress.stop
      >
      <button
        id="btn-join"
        :disabled="store.isConnecting"
        class="w-full bg-indigo-500 hover:bg-indigo-600 disabled:bg-slate-600 disabled:cursor-not-allowed disabled:opacity-60 text-white border-none py-3 rounded-lg font-semibold cursor-pointer transition-colors"
        @click="handleJoin"
      >
        ENTRAR NO JOGO
      </button>

      <div
        id="server-status"
        v-show="store.isConnecting"
        class="mt-6 flex flex-col items-center gap-3 fade-in"
      >
        <div class="spinner" />
        <span id="status-text" class="text-sm text-slate-400 font-medium">{{ store.serverMessage }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useGameStore } from '../stores/gameStore'

const store = useGameStore()
const playerName = ref('')

function handleJoin() {
  const name = playerName.value.trim()
  if (name) store.requestJoin(name)
}
</script>

<style scoped>
.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid rgba(99, 102, 241, 0.2);
  border-top: 3px solid #6366f1;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.fade-in {
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
