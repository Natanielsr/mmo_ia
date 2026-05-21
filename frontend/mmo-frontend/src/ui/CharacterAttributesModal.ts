export class CharacterAttributesModal {
    private overlay: HTMLElement;
    private isVisible: boolean = false;

    constructor() {
        const overlayElement = document.getElementById('attributes-modal-overlay');
        if (!overlayElement) {
            throw new Error('attributes-modal-overlay element not found in DOM');
        }
        this.overlay = overlayElement;
        this.overlay.style.display = 'none';
        this.isVisible = false;
        this.render();
        this.setupEventListeners();
    }

    private render() {
        this.overlay.innerHTML = `
            <div class="attributes-modal-content">
                <div class="attributes-modal-header">
                    <h2>Atributos</h2>
                    <button class="attributes-modal-close">✕</button>
                </div>
                <div class="attributes-modal-body">
                    <div class="attr-row attr-row-name">
                        <span class="attr-label">Nome:</span>
                        <span class="attr-value">-</span>
                    </div>
                    <div class="attr-row attr-row-level">
                        <span class="attr-label">Level:</span>
                        <span class="attr-value">1</span>
                    </div>
                    <div class="attr-row attr-row-hp">
                        <span class="attr-label">HP:</span>
                        <span class="attr-value">100 / 100</span>
                    </div>
                    <div class="attr-row attr-row-xp">
                        <span class="attr-label">XP:</span>
                        <span class="attr-value">0</span>
                    </div>
                    <div class="attr-row attr-row-attack">
                        <span class="attr-label">Ataque:</span>
                        <span class="attr-value">-</span>
                    </div>
                    <div class="attr-row attr-row-defense">
                        <span class="attr-label">Defesa:</span>
                        <span class="attr-value">-</span>
                    </div>
                </div>
            </div>
        `;
    }

    private setupEventListeners() {
        const closeBtn = this.overlay.querySelector('.attributes-modal-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', () => this.hide());
        }

        // Fechar modal ao clicar fora do conteúdo
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay) {
                this.hide();
            }
        });

        // Fechar com ESC
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isVisible) {
                this.hide();
            }
        });
    }

    public update(data: {
        name: string;
        level: number;
        hp: number;
        maxHp: number;
        experience: number;
        attackPower?: number;
        defense?: number;
    }) {
        const nameElement = this.overlay.querySelector('.attr-row-name .attr-value');
        if (nameElement && data.name !== undefined) nameElement.textContent = data.name;

        const levelElement = this.overlay.querySelector('.attr-row-level .attr-value');
        if (levelElement && data.level !== undefined) levelElement.textContent = String(data.level);

        const hpElement = this.overlay.querySelector('.attr-row-hp .attr-value');
        if (hpElement && data.hp !== undefined) hpElement.textContent = `${Math.ceil(data.hp)} / ${data.maxHp}`;

        const xpElement = this.overlay.querySelector('.attr-row-xp .attr-value');
        if (xpElement && data.experience !== undefined) xpElement.textContent = String(Math.ceil(data.experience));

        const attackElement = this.overlay.querySelector('.attr-row-attack .attr-value');
        if (attackElement && data.attackPower !== undefined) attackElement.textContent = String(data.attackPower);

        const defenseElement = this.overlay.querySelector('.attr-row-defense .attr-value');
        if (defenseElement && data.defense !== undefined) defenseElement.textContent = String(data.defense);
    }

    public show() {
        this.overlay.style.display = 'flex';
        this.isVisible = true;
    }

    public hide() {
        this.overlay.style.display = 'none';
        this.isVisible = false;
    }

    public toggle() {
        if (this.isVisible) {
            this.hide();
        } else {
            this.show();
        }
    }

    public destroy() {
        this.overlay.innerHTML = '';
    }
}
