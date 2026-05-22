<template>
  <div
    id="login-overlay"
    v-show="!store.isLoggedIn"
    class="overlay pointer-events-auto"
  >
    <div class="login-card glass">
      <h1>MMORPG Web</h1>
      <p>Entre no mundo e começe sua aventura</p>

      <div
        id="connection-error"
        v-show="store.connectionError"
        class="error-banner"
      >
        Não foi possível conectar ao servidor.
      </div>

      <input
        id="username"
        v-model="playerName"
        type="text"
        placeholder="Nome da personagem..."
        maxlength="12"
        @keydown.stop
        @keyup.stop
        @keypress.stop
      >
      <button
        id="btn-join"
        class="btn-primary"
        :disabled="store.isConnecting"
        @click="handleJoin"
      >
        ENTRAR NO JOGO
      </button>

      <div
        id="server-status"
        v-show="store.isConnecting"
        class="status-container"
      >
        <div class="spinner" />
        <span id="status-text">{{ store.serverMessage }}</span>
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
