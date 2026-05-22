export class CharacterStatusUI {
    private panel: HTMLElement;

    constructor() {
        const panelElement = document.getElementById('character-panel');
        if (!panelElement) {
            throw new Error('character-panel element not found in DOM');
        }
        this.panel = panelElement;
        this.render();
    }

    private render() {
        this.panel.innerHTML = `
            <h3 class="char-title">
                <span id="char-name">-</span> <span id="level-text" style="font-size: 0.8em; color: #ffd700;">Lv 1</span>
            </h3>
            <div class="hp-container">
                <div class="hp-bar progress-bar">
                    <div id="hp-fill" style="width: 100%"></div>
                </div>
                <div id="hp-text">100 / 100</div>
            </div>
            <div class="xp-container" style="margin-top: 5px;">
                <div class="xp-bar progress-bar" style="background: rgba(0, 0, 0, 0.5); border-radius: 4px; overflow: hidden; height: 12px; position: relative;">
                    <div id="xp-fill" style="width: 0%; height: 100%; background: #0088ff; transition: width 0.3s ease;"></div>
                </div>
                <div id="xp-text" style="font-size: 0.8em; text-align: center; margin-top: 2px;">0 / 1000 XP</div>
            </div>
            <div class="char-stats" style="margin-top: 5px; font-size: 0.85em;">
                <span>ATK: <span id="char-attack">-</span></span>
                <span style="margin-left: 8px;">DEF: <span id="char-defense">-</span></span>
            </div>
        `;
    }

    public setName(name: string) {
        const charName = document.getElementById('char-name');
        if (charName) {
            charName.textContent = name;
        }
    }

    public update(data: {
        hp: number;
        maxHp: number;
        level?: number;
        experience?: number;
        attackPower?: number;
        defense?: number;
    }) {
        // Update HP
        const currentHp = Number(data.hp ?? 0);
        const totalHp = Number(data.maxHp ?? 100);
        const hpPercent = Math.max(0, (currentHp / (totalHp || 1)) * 100);

        const hpFill = document.getElementById('hp-fill') as HTMLElement;
        if (hpFill) {
            hpFill.style.width = `${hpPercent}%`;
        }

        const hpText = document.getElementById('hp-text');
        if (hpText) {
            hpText.textContent = `${Math.ceil(currentHp)} / ${totalHp}`;
        }

        if (data.level !== undefined && data.experience !== undefined) {
            const levelText = document.getElementById('level-text');
            if (levelText) {
                levelText.textContent = `Lv ${data.level}`;
            }

            const prevLevelMaxXP = (data.level - 1) * 1000;
            const currentLevelMaxXP = data.level * 1000;
            const xpIntoLevel = Math.max(0, data.experience - prevLevelMaxXP);
            const xpRequiredForNextLevel = currentLevelMaxXP - prevLevelMaxXP;
            const xpPercent = Math.min(100, Math.max(0, (xpIntoLevel / xpRequiredForNextLevel) * 100));

            const xpFill = document.getElementById('xp-fill') as HTMLElement;
            if (xpFill) {
                xpFill.style.width = `${xpPercent}%`;
            }

            const xpText = document.getElementById('xp-text');
            if (xpText) {
                xpText.textContent = `${Math.ceil(xpIntoLevel)} / ${xpRequiredForNextLevel} XP`;
            }
        }

        // Update Attack Power
        const attackElement = document.getElementById('char-attack');
        if (attackElement) {
            attackElement.textContent = data.attackPower !== undefined ? String(data.attackPower) : '-';
        }

        // Update Defense
        const defenseElement = document.getElementById('char-defense');
        if (defenseElement) {
            defenseElement.textContent = data.defense !== undefined ? String(data.defense) : '-';
        }
    }

    public destroy() {
        this.panel.innerHTML = '';
    }
}
