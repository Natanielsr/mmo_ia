export class GameLogUI {
    private logContent: HTMLDivElement;

    constructor() {
        const footer = document.getElementById('game-log-footer');
        if (!footer) throw new Error('game-log-footer element not found');

        const title = document.createElement('h3');
        title.textContent = 'Log de Atividades';

        this.logContent = document.createElement('div');
        this.logContent.id = 'log-content';

        footer.appendChild(title);
        footer.appendChild(this.logContent);
    }

    public addEntry(message: string, isError: boolean = false): void {
        const entry = document.createElement('div');
        entry.classList.add('log-entry');
        if (isError) entry.classList.add('error');
        entry.textContent = message;

        this.logContent.insertBefore(entry, this.logContent.firstChild);

        if (this.logContent.children.length > 100) {
            this.logContent.removeChild(this.logContent.lastChild as HTMLElement);
        }
    }

    public clear(): void {
        this.logContent.innerHTML = '';
    }
}
