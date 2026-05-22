import './css/style.css';
import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './components/App.vue';
import Phaser from 'phaser';
import { PreloadScene } from './scenes/PreloadScene';
import { MainScene } from './scenes/MainScene';
import { WorldMapScene } from './scenes/WorldMapScene';
import { SignalRService } from './services/SignalRService';
import { gameContainer } from './ui';
import { useGameStore } from './stores/gameStore';

// Mount Vue app before Phaser so Pinia is available to all modules
const pinia = createPinia()
const vueApp = createApp(App)
vueApp.use(pinia)
vueApp.mount('#vue-ui')

// Inicializa o Phaser
const phaserConfig: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: 'phaser-game',
  width: '100%',
  height: '100%',
  pixelArt: true,
  backgroundColor: '#1e293b',
  scene: [PreloadScene, MainScene, WorldMapScene]
};
const game = new Phaser.Game(phaserConfig);

// Inicializa o Serviço SignalR
const signalRService = new SignalRService();
useGameStore().setJoinCallback((name) => signalRService.invoke("JoinGame", name));

// Register SignalR events. We need to handle the fact that scenes might not be initialized yet.
// However, signalRService.registerEvents currently expects the instances.
// Let's modify MainScene to register itself or similar, but for now we'll stick to a slightly safer approach.
// Since PreloadScene starts MainScene immediately, we can wait a bit or use events.
game.events.once('ready', () => {
    const mainScene = game.scene.getScene('MainScene') as MainScene;
    mainScene.setSignalRService(signalRService);
    signalRService.registerEvents(mainScene, game);
});

window.addEventListener('resize', () => {
  if (!gameContainer.classList.contains('hidden')) {
    const container = document.getElementById('phaser-game');
    if (container) game.scale.resize(container.clientWidth, container.clientHeight);
  }
});

// Inicia a aplicação
signalRService.start();
