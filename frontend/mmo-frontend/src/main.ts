import './css/style.css';
import Phaser from 'phaser';
import { PreloadScene } from './scenes/PreloadScene';
import { MainScene } from './scenes/MainScene';
import { WorldMapScene } from './scenes/WorldMapScene';
import { SignalRService } from './services/SignalRService';
import { inputName, btnJoin, gameContainer } from './ui';

// Impede que o Phaser capture as teclas enquanto o utilizador digita no input
inputName.addEventListener('keydown', (e) => e.stopPropagation());
inputName.addEventListener('keyup', (e) => e.stopPropagation());
inputName.addEventListener('keypress', (e) => e.stopPropagation());

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

// Register SignalR events. We need to handle the fact that scenes might not be initialized yet.
// However, signalRService.registerEvents currently expects the instances.
// Let's modify MainScene to register itself or similar, but for now we'll stick to a slightly safer approach.
// Since PreloadScene starts MainScene immediately, we can wait a bit or use events.
game.events.once('ready', () => {
    const mainScene = game.scene.getScene('MainScene') as MainScene;
    signalRService.registerEvents(mainScene, game);
});

btnJoin.onclick = () => {
  const name = inputName.value.trim();
  if (name) signalRService.invoke("JoinGame", name);
};

window.addEventListener('resize', () => {
  if (!gameContainer.classList.contains('hidden')) {
    const container = document.getElementById('phaser-game');
    if (container) game.scale.resize(container.clientWidth, container.clientHeight);
  }
});

// Inicia a aplicação
signalRService.start();
