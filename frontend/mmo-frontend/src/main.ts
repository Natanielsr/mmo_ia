import './css/style.css';
import Phaser from 'phaser';
import * as signalR from '@microsoft/signalr';
import { PreloadScene } from './game/PreloadScene';
import { MainScene } from './game/MainScene';
import type { PlayerData, MonsterData } from './types';

// Elementos da UI
const overlay = document.getElementById('login-overlay') as HTMLDivElement;
const gameContainer = document.getElementById('game-container') as HTMLDivElement;
const btnJoin = document.getElementById('btn-join') as HTMLButtonElement;
const inputName = document.getElementById('username') as HTMLInputElement;
const logContent = document.getElementById('log-content') as HTMLDivElement;
const errorBanner = document.getElementById('connection-error') as HTMLDivElement;

function updateUIHealthBar(hp: any, maxHp: any) {
  // Garantir que temos números válidos
  const currentHp = Number(hp ?? 0);
  const totalHp = Number(maxHp ?? 100);

  const percent = Math.max(0, (currentHp / (totalHp || 1)) * 100);

  const fill = document.getElementById('hp-fill');
  const text = document.getElementById('hp-text');

  if (fill) {
    fill.style.width = isNaN(percent) ? "0%" : `${percent}%`;
  } else {
    console.error("hp-fill element NOT FOUND in DOM!");
  }

  if (text) {
    text.innerText = `${isNaN(currentHp) ? 0 : Math.ceil(currentHp)} / ${isNaN(totalHp) ? 100 : totalHp}`;
  } else {
    console.error("hp-text element NOT FOUND in DOM!");
  }
}

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
connection.on("Joined", (playerData: any) => {
  mainScene.loadMap();
  addLog(`Entraste como ${playerData.name}!`);
  overlay.classList.add('hidden');
  gameContainer.classList.remove('hidden');

  // Força o redimensionamento do canvas
  const container = document.getElementById('phaser-game');
  if (container) game.scale.resize(container.clientWidth, container.clientHeight);

  mainScene.updatePlayerPosition(playerData, true);
  updateUIHealthBar(playerData.hp ?? playerData.Hp, playerData.maxHp ?? playerData.MaxHp);
});

connection.on("SyncPlayers", (playerList: any[]) => {
  playerList.forEach(p => {
    if (p.position && (p.position.x !== undefined || p.position.X !== undefined)) {
      mainScene.updatePlayerPosition(p);
      if (mainScene.myId === p.id.toString()) {
        updateUIHealthBar(p.hp ?? p.Hp, p.maxHp ?? p.MaxHp);
      }
    }
  });
});

connection.on("PlayerJoined", (playerData: PlayerData) => {
  addLog(`${playerData.name} entrou no mundo!`);
  if (playerData.position.x !== undefined && playerData.position.y !== undefined) {
    mainScene.updatePlayerPosition(playerData);
  }
});

connection.on("PlayerMoved", (playerData: any) => {
  mainScene.updatePlayerPosition(playerData);
  if (mainScene.myId === playerData.id.toString()) {
    updateUIHealthBar(playerData.hp ?? playerData.Hp, playerData.maxHp ?? playerData.MaxHp);
  }
});

connection.on("PlayerLeft", (playerId: number) => {
  addLog(`${playerId} saiu do mundo.`);
  mainScene.removePlayer(playerId);
});

connection.on("PlayerAttacked", (data: any) => {
  const attacker = data.attackerId ?? data.AttackerId;
  const target = data.targetId ?? data.TargetId;
  const dmg = data.damage ?? data.Damage;

  mainScene.playerAttacked({ attackerId: attacker, targetId: target, damage: dmg });
  addLog(`${attacker} atacou ${target} causando ${dmg} de dano!`);

  const myPlayer = mainScene.getMyPlayer();
  if (myPlayer) {
    const targetIdStr = String(target);
    const myIdStr = String(myPlayer.id);

    if (targetIdStr === myIdStr) {
      updateUIHealthBar(myPlayer.hp, myPlayer.maxHp);
    }
  }
});

connection.on("PlayerDied", (playerId: number) => {
  mainScene.playerDied(playerId);
  addLog(`O jogador ${playerId} morreu!`, "error");

  if (mainScene.myId === playerId.toString()) {
    updateUIHealthBar(0, 100); // Or get maxHp from somewhere
  }
});

// --- Eventos de Monstros ---
connection.on("SyncMonsters", (monsterList: MonsterData[]) => {
  console.log(monsterList);
  mainScene.syncMonsters(monsterList);
  addLog(`Sincronizados ${monsterList.length} monstros no mundo.`, "info");
});

connection.on("MonsterSpawned", (monsterData: MonsterData) => {
  mainScene.monsterSpawned(monsterData);
  addLog(`Monstro ${monsterData.name} apareceu!`, "info");
});

connection.on("MonsterMoved", (monsterData: MonsterData) => {
  mainScene.monsterMoved(monsterData);
});

connection.on("MonsterDied", (monsterId: number) => {
  mainScene.monsterDied(monsterId);
  addLog(`Monstro ID ${monsterId} foi derrotado!`, "success");
});

connection.on("MonsterDamaged", (data: { monsterId: number; damage: number; currentHp: number }) => {
  mainScene.monsterDamaged(data);
  addLog(`Monstro ID ${data.monsterId} sofreu ${data.damage} de dano!`, "info");
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
