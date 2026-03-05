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
const errorBanner = document.getElementById('connection-error');

const WORLD_SIZE = 2048;
const GRID_SIZE = 32;
const WORLD_CENTER = WORLD_SIZE / 2; // 1024, exactly 32 * 32

// --- SignalR Lifecycle ---

connection.onreconnecting((error) => {
    console.warn("Reconectando...", error);
    errorBanner.classList.remove('hidden');
    addLog("Conexão perdida. Tentando reconectar...", "error");
    btnJoin.disabled = true;
});

connection.onreconnected((connectionId) => {
    console.log("Reconectado!");
    errorBanner.classList.add('hidden');
    addLog("Conexão reestabelecida!", "info");
    btnJoin.disabled = false;
});

connection.onclose((error) => {
    console.error("Conexão fechada!", error);
    errorBanner.innerText = "Conexão perdida. Por favor, recarregue a página.";
    errorBanner.classList.remove('hidden');
    addLog("Conexão encerrada. Por favor, recarregue a página.", "error");
    btnJoin.disabled = true;
});

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
        btnJoin.disabled = true;
        await connection.start();
        console.log("Conectado ao SignalR!");
        errorBanner.classList.add('hidden');
        btnJoin.disabled = false;
    } catch (err) {
        console.error("Erro ao conectar:", err);
        addLog("Não foi possível conectar ao servidor. Tentando novamente...", "error");
        errorBanner.innerText = "Conexão perdida. Por favor, recarregue a página.";
        errorBanner.classList.remove('hidden');

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
    let playerElement = document.getElementById(`player-${id}`);
    if (!playerElement) {
        playerElement = document.createElement('div');
        playerElement.id = `player-${id}`;
        playerElement.className = 'player-token';
        playerElement.setAttribute('data-name', id);
        if (isMe) playerElement.style.backgroundColor = '#818cf8';
        worldMap.appendChild(playerElement);
    }

    // Fixed world coordinates
    const px = WORLD_CENTER + (x * GRID_SIZE);
    const py = WORLD_CENTER - (y * GRID_SIZE);

    playerElement.style.left = `${px}px`;
    playerElement.style.top = `${py}px`;

    if (isMe) {
        // Center camera: viewport center - player world position
        const viewX = (gameContainer.offsetWidth / 2) - px;
        const viewY = (gameContainer.offsetHeight / 2) - py;
        worldMap.style.transform = `translate(${viewX}px, ${viewY}px)`;
    }
}

function createGridLabels() {
    const range = 20; // Generate labels around the center
    for (let x = -range; x <= range; x++) {
        for (let y = -range; y <= range; y++) {
            const label = document.createElement('div');
            label.className = 'grid-label';
            label.innerText = `${x},${-y}`;
            label.style.left = `${WORLD_CENTER + (x * GRID_SIZE)}px`;
            label.style.top = `${WORLD_CENTER + (y * GRID_SIZE)}px`;
            worldMap.appendChild(label);
        }
    }
}

createGridLabels();

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
