// Elementos da UI
export const overlay = document.getElementById('login-overlay') as HTMLDivElement;
export const gameContainer = document.getElementById('game-container') as HTMLDivElement;
export const btnJoin = document.getElementById('btn-join') as HTMLButtonElement;
export const inputName = document.getElementById('username') as HTMLInputElement;
export const logContent = document.getElementById('log-content') as HTMLDivElement;
export const errorBanner = document.getElementById('connection-error') as HTMLDivElement;

export function updateUIHealthBar(hp: any, maxHp: any) {
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

export function addLog(msg: string, type: string = '') {
    const entry = document.createElement('div');
    entry.className = `log-entry ${type}`;
    entry.innerText = `[${new Date().toLocaleTimeString()}] ${msg}`;
    if (logContent) {
        logContent.prepend(entry);
    }
}
