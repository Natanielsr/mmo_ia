import { SignalRService } from '../services/SignalRService';

export class DevConsole {
    private panel: HTMLDivElement | null = null;
    private input: HTMLInputElement | null = null;
    private output: HTMLDivElement | null = null;
    private isDevelopment: boolean = import.meta.env.DEV;
    private signalRService: SignalRService;

    constructor(signalRService: SignalRService) {
        this.signalRService = signalRService;
        if (!this.isDevelopment) {
            return;
        }

        this.panel = document.createElement('div');
        this.panel.className = 'dev-console hidden';

        this.output = document.createElement('div');
        this.output.className = 'console-output';
        this.panel.appendChild(this.output);

        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.className = 'console-input';
        this.input.placeholder = 'Comando de cheat (/help para ajuda)';
        this.input.addEventListener('keydown', (e) => {
            e.stopPropagation();
            this.handleCommand(e);
        });
        this.input.addEventListener('keyup', (e) => e.stopPropagation());
        this.input.addEventListener('keypress', (e) => e.stopPropagation());
        this.panel.appendChild(this.input);

        document.body.appendChild(this.panel);
        this.log('Console de Desenvolvedor carregado. Digite /help para ajuda.');
    }

    public toggle(): void {
        if (!this.isDevelopment || !this.panel) return;
        this.panel.classList.toggle('hidden');
        if (!this.panel.classList.contains('hidden') && this.input) {
            this.input.focus();
        }
    }

    private handleCommand(e: KeyboardEvent): void {
        if (e.key === 'Escape') {
            this.toggle();
            return;
        }

        if (e.key !== 'Enter' || !this.input) return;

        const command = this.input.value.trim().toLowerCase();
        this.input.value = '';

        if (!command) return;

        this.log(`> ${command}`);
        this.executeCommand(command);
    }

    private async executeCommand(command: string): Promise<void> {
        const parts = command.split(/\s+/);
        const cmd = parts[0];

        try {
            if (cmd === '/help') {
                this.showHelp();
            } else if (cmd === '/clear') {
                if (this.output) this.output.innerHTML = '';
            } else if (cmd === '/exit') {
                this.toggle();
            } else if (cmd === '/spawn' && parts[1]) {
                const tagName = parts[1].toLowerCase();
                await this.signalRService.invoke('DevSpawnItemByTag', tagName);
                this.log(`✓ Item spawned: ${tagName}`);
            } else if (cmd === '/level' && parts[1]) {
                const level = parseInt(parts[1], 10);
                if (isNaN(level) || level < 1) {
                    this.log('✗ Nível inválido. Use: /level <1-999>');
                    return;
                }
                await this.signalRService.invoke('DevSetLevel', level);
                this.log(`✓ Nível definido para ${level}`);
            } else if (cmd === '/god') {
                await this.signalRService.invoke('DevGodMode', true);
                this.log('✓ God mode ATIVADO');
            } else if (cmd === '/mortal') {
                await this.signalRService.invoke('DevGodMode', false);
                this.log('✓ God mode DESATIVADO');
            } else if (cmd === '/heal') {
                await this.signalRService.invoke('DevFullHeal');
                this.log('✓ HP total restaurado');
            } else if (cmd === '/kill') {
                await this.signalRService.invoke('DevOneHitKill');
                this.log('✓ Monstros próximos mortos');
            } else if (cmd === '/allitems') {
                await this.signalRService.invoke('DevGiveAllItems');
                this.log('✓ Todos os itens spawned no inventário');
            } else if (cmd === '/speed' && parts[1]) {
                const speed = parseFloat(parts[1]);
                if (isNaN(speed) || speed <= 0) {
                    this.log('✗ Velocidade inválida. Use: /speed <1-10>');
                    return;
                }
                await this.signalRService.invoke('DevSetSpeed', speed);
                this.log(`✓ Velocidade definida para ${Math.min(Math.max(speed, 1), 10).toFixed(1)} (padrão: 2.0)`);
            } else if (cmd === '/speed') {
                this.log('✗ Use: /speed <valor> (1.0–10.0, padrão 2.0)');
            } else {
                this.log('✗ Comando desconhecido. Digite /help para ajuda.');
            }
        } catch (err) {
            this.log(`✗ Erro: ${err}`);
        }
    }

    private showHelp(): void {
        const help = [
            '─ COMANDOS DE CHEAT ─',
            '/spawn <tagName>  → Spawna item no chão (ex: plate-armor, dagger, potion)',
            '/level <n>        → Define nível (1-999)',
            '/speed <n>        → Define velocidade (1.0–10.0, padrão 2.0)',
            '/god              → Ativa modo deus (invencível)',
            '/mortal           → Desativa modo deus',
            '/heal             → Restaura HP total',
            '/kill             → Mata monstros próximos (raio 5)',
            '/allitems         → Spawna um de cada item no inventário',
            '/clear            → Limpa o console',
            '/exit             → Fecha o console',
            '/help             → Mostra esta mensagem',
            '─ Teclas ─',
            'ESC               → Fecha o console',
            '~ (til)           → Abre/fecha o console',
            '─ Exemplos ─',
            '/spawn plate-armor',
            '/level 10',
            '/speed 5',
            '/god'
        ];
        help.forEach(line => this.log(line));
    }

    private log(message: string): void {
        if (!this.output) return;
        const entry = document.createElement('div');
        entry.className = 'console-entry';
        entry.textContent = message;
        this.output.appendChild(entry);
        this.output.scrollTop = this.output.scrollHeight;
    }

    public destroy(): void {
        if (this.isDevelopment && this.panel) {
            this.panel.remove();
        }
    }
}
