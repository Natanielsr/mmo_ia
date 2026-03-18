import type { Monster } from './Monster';

export class DebugPanel {
    private container: HTMLDivElement;
    private content: HTMLDivElement;
    private isVisible: boolean = false;
    private isDevelopment: boolean = import.meta.env.DEV;

    constructor() {
        this.container = document.createElement('div');
        this.container.className = 'debug-panel hidden';

        const title = document.createElement('h2');
        title.innerText = 'Debug Mode (F3)';
        this.container.appendChild(title);

        const badge = document.createElement('div');
        badge.className = 'debug-badge';
        badge.innerText = 'DEV';
        this.container.appendChild(badge);

        this.content = document.createElement('div');
        this.container.appendChild(this.content);

        document.body.appendChild(this.container);
    }

    public toggle(): void {
        if (!this.isDevelopment) return;

        this.isVisible = !this.isVisible;
        if (this.isVisible) {
            this.container.classList.remove('hidden');
        } else {
            this.container.classList.add('hidden');
        }
    }

    public update(monsters: Record<string, Monster>): void {
        if (!this.isVisible || !this.isDevelopment) return;

        const monsterList = Object.values(monsters);
        const aliveMonsters = monsterList.filter(m => !m.isDead);

        let html = `
            <div>Total: ${monsterList.length}</div>
            <div>Vivos: ${aliveMonsters.length}</div>
            <br>
        `;

        aliveMonsters.forEach(m => {
            html += `
                <div class="debug-monster-item">
                    <span><strong>${m.name}</strong> [${m.id}]</span>
                    <span>Pos: (${m.gridPosition.x}, ${m.gridPosition.y})</span>
                    <span class="hp-text">HP: ${m.hp} / ${m.maxHp}</span>
                </div>
            `;
        });

        this.content.innerHTML = html;
    }

    public destroy(): void {
        if (this.container && this.container.parentElement) {
            this.container.parentElement.removeChild(this.container);
        }
    }
}
