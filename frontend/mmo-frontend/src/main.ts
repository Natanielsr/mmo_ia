import './css/style.css';
import Phaser from 'phaser';
import { PreloadScene } from './scenes/PreloadScene';
import { MainScene } from './scenes/MainScene';
import { SignalRService } from './services/SignalRService';
import { inputName, btnJoin, gameContainer } from './ui';

// Impede que o Phaser capture as teclas enquanto o utilizador digita no input
inputName.addEventListener('keydown', (e) => e.stopPropagation());
inputName.addEventListener('keyup', (e) => e.stopPropagation());
inputName.addEventListener('keypress', (e) => e.stopPropagation());

// Inicializa o Phaser
const preloadScene = new PreloadScene();
const mainScene = new MainScene();
const phaserConfig: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: 'phaser-game',
  width: '100%',
  height: '100%',
  pixelArt: true,
  backgroundColor: '#1e293b',
  scene: [preloadScene, mainScene]
};
const game = new Phaser.Game(phaserConfig);

// Inicializa o Serviço SignalR
const signalRService = new SignalRService();
signalRService.registerEvents(mainScene, game);

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
