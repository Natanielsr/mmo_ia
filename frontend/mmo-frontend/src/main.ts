import './css/style.css'; // O Vite lida com a importação de CSS automaticamente
import Phaser from 'phaser';
import * as signalR from '@microsoft/signalr';
import { MainScene } from './game/MainScene';
import type { PlayerData, AttackData } from './types';

// Elementos da UI
const overlay = document.getElementById('login-overlay') as HTMLDivElement;
const gameContainer = document.getElementById('game-container') as HTMLDivElement;
const btnJoin = document.getElementById('btn-join') as HTMLButtonElement;
const inputName = document.getElementById('username') as HTMLInputElement;
const logContent = document.getElementById('log-content') as HTMLDivElement;
const errorBanner = document.getElementById('connection-error') as HTMLDivElement;

// Impede que o Phaser capture as teclas enquanto o utilizador digita no input
inputName.addEventListener('keydown', (e) => e.stopPropagation());
inputName.addEventListener('keyup', (e) => e.stopPropagation());
inputName.addEventListener('keypress', (e) => e.stopPropagation());

// Inicializa o Phaser
const mainScene = new MainScene();
const phaserConfig: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: 'phaser-game',
  width: '100%',
  height: '100%',
  pixelArt: true,
  backgroundColor: '#1e293b',
  scene: [mainScene]
};
const game = new Phaser.Game(phaserConfig);

// Conexão SignalR
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5258/gamehub")
  .withAutomaticReconnect()
  .build();

// Conecta o evento de input do Phaser ao envio do SignalR
mainScene.onRequestMove = (direction: string) => {
  connection.invoke("RequestMove", direction).catch(console.error);
};

// --- Eventos do SignalR ---
connection.on("Joined", (playerData: PlayerData) => {
  addLog(`Entraste como ${playerData.name}!`);
  overlay.classList.add('hidden');
  gameContainer.classList.remove('hidden');

  // Força o redimensionamento do canvas
  const container = document.getElementById('phaser-game');
  if (container) game.scale.resize(container.clientWidth, container.clientHeight);

  mainScene.updatePlayerPosition(playerData, true);
});

connection.on("SyncPlayers", (playerList: PlayerData[]) => {
  playerList.forEach(p => {
    if (p.position.x !== undefined && p.position.y !== undefined) {
      mainScene.updatePlayerPosition(p);
    }
  });
});

connection.on("PlayerJoined", (playerData: PlayerData) => {
  addLog(`${playerData.name} entrou no mundo!`);
  if (playerData.position.x !== undefined && playerData.position.y !== undefined) {
    mainScene.updatePlayerPosition(playerData);
  }
});

connection.on("PlayerMoved", (playerData: PlayerData) => {
  mainScene.updatePlayerPosition(playerData);

});

connection.on("PlayerLeft", (playerId: string) => {
  addLog(`${playerId} saiu do mundo.`);
  mainScene.removePlayer(playerId);
});

connection.on("PlayerAttacked", (data: AttackData) => {
  addLog(`${data.attackerId} atacou ${data.targetId} causando ${data.damage} de dano!`);
});

// Funções de UI
function addLog(msg: string, type: string = '') {
  const entry = document.createElement('div');
  entry.className = `log-entry ${type}`;
  entry.innerText = `[${new Date().toLocaleTimeString()}] ${msg}`;
  logContent.prepend(entry);
}

async function start() {
  try {
    btnJoin.disabled = true;
    await connection.start();
    console.log("Conectado ao SignalR!");
    errorBanner.classList.add('hidden');
    btnJoin.disabled = false;
  } catch (err) {
    addLog("Não foi possível conectar ao servidor. A tentar novamente...", "error");
    errorBanner.classList.remove('hidden');
    setTimeout(start, 5000);
  }
}

btnJoin.onclick = () => {
  const name = inputName.value.trim();
  if (name) connection.invoke("JoinGame", name);
};

window.addEventListener('resize', () => {
  if (!gameContainer.classList.contains('hidden')) {
    const container = document.getElementById('phaser-game');
    if (container) game.scale.resize(container.clientWidth, container.clientHeight);
  }
});

// Inicia a aplicação
start();