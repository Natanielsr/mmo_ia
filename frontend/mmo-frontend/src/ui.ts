// Elementos da UI
export const overlay = document.getElementById('login-overlay') as HTMLDivElement;
export const gameContainer = document.getElementById('game-container') as HTMLDivElement;
export const btnJoin = document.getElementById('btn-join') as HTMLButtonElement;
export const inputName = document.getElementById('username') as HTMLInputElement;
export const logContent = document.getElementById('log-content') as HTMLDivElement;
export const errorBanner = document.getElementById('connection-error') as HTMLDivElement;
export const serverStatus = document.getElementById('server-status') as HTMLDivElement;
export const statusText = document.getElementById('status-text') as HTMLSpanElement;

export function updateServerStatus(msg: string, isVisible: boolean) {
    if (serverStatus) {
        if (isVisible) serverStatus.classList.remove('hidden');
        else serverStatus.classList.add('hidden');
    }
    if (statusText) statusText.innerText = msg;
}
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

export function updateUIXPBar(experience: number, level: number) {
    const currentExp = Number(experience ?? 0);
    const lvl = Number(level ?? 1);
    
    // Formula from backend: Level * 1000
    // At level 1, you need 1000 total XP to reach level 2. 
    // At level 2, you need 2000 total XP to reach level 3.
    // The previous level amount was (lvl - 1) * 1000
    const prevLevelMaxXP = (lvl - 1) * 1000;
    const currentLevelMaxXP = lvl * 1000;
    
    // How much XP into this level we have
    const xpIntoLevel = Math.max(0, currentExp - prevLevelMaxXP);
    const xpRequiredForNextLevel = currentLevelMaxXP - prevLevelMaxXP; // This is always 1000
    
    const percent = Math.min(100, Math.max(0, (xpIntoLevel / xpRequiredForNextLevel) * 100));

    const fill = document.getElementById('xp-fill');
    const text = document.getElementById('xp-text');
    const levelText = document.getElementById('level-text');

    if (levelText) {
        levelText.innerText = `Lv ${lvl}`;
    }

    if (fill) {
        fill.style.width = isNaN(percent) ? "0%" : `${percent}%`;
    }

    if (text) {
        text.innerText = `${Math.ceil(xpIntoLevel)} / ${xpRequiredForNextLevel} XP`;
    }
}

export function updateUIPosition(x: number, y: number) {
    const posX = document.getElementById('pos-x');
    const posY = document.getElementById('pos-y');
    if (posX) posX.innerText = String(x);
    if (posY) posY.innerText = String(y);
}

export function addLog(msg: string, type: string = '') {
    const entry = document.createElement('div');
    entry.className = `log-entry ${type}`;
    entry.innerText = `[${new Date().toLocaleTimeString()}] ${msg}`;
    if (logContent) {
        logContent.prepend(entry);
    }
}
