const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5258/gamehub")
    .withAutomaticReconnect()
    .build();

const players = {};
let myId = null;

// Game elements
const overlay = document.getElementById('login-overlay');
const gameContainer = document.getElementById('game-container');
const btnJoin = document.getElementById('btn-join');
const inputName = document.getElementById('username');
const worldMap = document.getElementById('world-map');
const logContent = document.getElementById('log-content');

// --- SignalR Handlers ---

connection.on("Joined", (data) => {
    addLog(`Você entrou como ${data.name}!`);
    overlay.classList.add('hidden');
    gameContainer.classList.remove('hidden');

    // Create/update local player representation
    updatePlayerPosition(data.name, data.position.x, data.position.y, true);
});

connection.on("SyncPlayers", (playerList) => {
    playerList.forEach(p => {
        updatePlayerPosition(p.playerId, p.x, p.y);
    });
});

connection.on("PlayerJoined", (data) => {
    addLog(`${data.playerId} entrou no mundo!`);
    updatePlayerPosition(data.playerId, data.x, data.y);
});

connection.on("PlayerMoved", (data) => {
    updatePlayerPosition(data.playerId, data.x, data.y);
});

connection.on("PlayerLeft", (playerId) => {
    addLog(`${playerId} saiu do mundo.`);
    removePlayer(playerId);
});

connection.on("PlayerAttacked", (data) => {
    addLog(`${data.attackerId} atacou ${data.targetId} causando ${data.damage} de dano!`);
});

connection.on("MoveFailed", (msg) => {
    addLog(`Sistema: ${msg}`, 'error');
});

// --- Actions ---

async function start() {
    try {
        await connection.start();
        console.log("Conectado ao SignalR!");
    } catch (err) {
        console.error("Erro ao conectar:", err);
        setTimeout(start, 5000);
    }
}

btnJoin.onclick = () => {
    const name = inputName.value.trim();
    if (name) {
        connection.invoke("JoinGame", name);
    }
};

// Movement handling
window.onkeydown = (e) => {
    if (gameContainer.classList.contains('hidden')) return;

    let direction = null;
    switch (e.key.toLowerCase()) {
        case 'arrowup':
        case 'w': direction = "north"; break;
        case 'arrowdown':
        case 's': direction = "south"; break;
        case 'arrowleft':
        case 'a': direction = "west"; break;
        case 'arrowright':
        case 'd': direction = "east"; break;
    }

    if (direction) {
        connection.invoke("RequestMove", direction);
    }
};

// --- Utils ---

function updatePlayerPosition(id, x, y, isMe = false) {
    let el = document.getElementById(`player-${id}`);
    if (!el) {
        el = document.createElement('div');
        el.id = `player-${id}`;
        el.className = 'player-token';
        el.setAttribute('data-name', id);
        if (isMe) el.style.backgroundColor = '#818cf8';
        worldMap.appendChild(el);
    }

    // Convert coordinates to pixels (simple 32px grid)
    const px = x * 32 + (window.innerWidth / 2) - 160;
    const py = y * 32 + (window.innerHeight / 2) - 100;

    el.style.left = `${px}px`;
    el.style.top = `${py}px`;

    if (isMe) {
        // Center camera pseudo-logic
        worldMap.style.transform = `translate(${-x * 32}px, ${-y * 32}px)`;
    }
}

function removePlayer(id) {
    const el = document.getElementById(`player-${id}`);
    if (el) {
        el.remove();
    }
}

function addLog(msg, type = '') {
    const entry = document.createElement('div');
    entry.className = `log-entry ${type}`;
    entry.innerText = `[${new Date().toLocaleTimeString()}] ${msg}`;
    logContent.prepend(entry);
}

start();
