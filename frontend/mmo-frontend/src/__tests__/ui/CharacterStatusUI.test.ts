import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { CharacterStatusUI } from '../../ui/CharacterStatusUI';

describe('CharacterStatusUI', () => {
    let ui: CharacterStatusUI;

    beforeEach(() => {
        document.body.innerHTML = '<div id="character-panel"></div>';
        ui = new CharacterStatusUI();
    });

    afterEach(() => {
        ui.destroy();
    });

    describe('constructor', () => {
        it('cria elemento #char-name', () => {
            const charName = document.getElementById('char-name');
            expect(charName).toBeTruthy();
        });

        it('cria #hp-fill, #hp-text, #xp-fill, #xp-text, #level-text', () => {
            expect(document.getElementById('hp-fill')).toBeTruthy();
            expect(document.getElementById('hp-text')).toBeTruthy();
            expect(document.getElementById('xp-fill')).toBeTruthy();
            expect(document.getElementById('xp-text')).toBeTruthy();
            expect(document.getElementById('level-text')).toBeTruthy();
        });

        it('cria #char-attack e #char-defense', () => {
            expect(document.getElementById('char-attack')).toBeTruthy();
            expect(document.getElementById('char-defense')).toBeTruthy();
        });

        it('inicializa #char-attack e #char-defense com "-"', () => {
            const attack = document.getElementById('char-attack');
            const defense = document.getElementById('char-defense');
            expect(attack?.textContent).toBe('-');
            expect(defense?.textContent).toBe('-');
        });
    });

    describe('setName', () => {
        it('exibe o nome do personagem em #char-name', () => {
            ui.setName('Nataniel');
            const charName = document.getElementById('char-name');
            expect(charName?.textContent).toBe('Nataniel');
        });

        it('atualiza o nome quando chamado múltiplas vezes', () => {
            ui.setName('Heroe');
            let charName = document.getElementById('char-name');
            expect(charName?.textContent).toBe('Heroe');

            ui.setName('NovoNome');
            charName = document.getElementById('char-name');
            expect(charName?.textContent).toBe('NovoNome');
        });
    });

    describe('update', () => {
        it('atualiza hp e maxHp corretamente', () => {
            ui.update({ hp: 85, maxHp: 100, level: 1, experience: 0 });
            const hpText = document.getElementById('hp-text');
            expect(hpText?.textContent).toBe('85 / 100');
        });

        it('calcula a largura da barra de HP corretamente', () => {
            ui.update({ hp: 50, maxHp: 100, level: 1, experience: 0 });
            const hpFill = document.getElementById('hp-fill') as HTMLElement;
            expect(hpFill?.style.width).toBe('50%');
        });

        it('atualiza level em #level-text', () => {
            ui.update({ hp: 100, maxHp: 100, level: 5, experience: 4000 });
            const levelText = document.getElementById('level-text');
            expect(levelText?.textContent).toBe('Lv 5');
        });

        it('atualiza xp bar com percentual correto', () => {
            // Level 1: prevMax = 0, currentMax = 1000
            // XP = 500 → 50% into level
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 500 });
            const xpFill = document.getElementById('xp-fill') as HTMLElement;
            expect(xpFill?.style.width).toBe('50%');

            const xpText = document.getElementById('xp-text');
            expect(xpText?.textContent).toBe('500 / 1000 XP');
        });

        it('exibe attackPower quando presente', () => {
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 0, attackPower: 15 });
            const attack = document.getElementById('char-attack');
            expect(attack?.textContent).toBe('15');
        });

        it('exibe defense quando presente', () => {
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 0, defense: 8 });
            const defense = document.getElementById('char-defense');
            expect(defense?.textContent).toBe('8');
        });

        it('exibe "-" quando attackPower ausente', () => {
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 0 });
            const attack = document.getElementById('char-attack');
            expect(attack?.textContent).toBe('-');
        });

        it('exibe "-" quando defense ausente', () => {
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 0 });
            const defense = document.getElementById('char-defense');
            expect(defense?.textContent).toBe('-');
        });

        it('calcula XP corretamente para level > 1', () => {
            // Level 2: prevMax = 1000, currentMax = 2000
            // XP = 1500 → 500 XP into level 2 → 50% (500 / 1000)
            ui.update({ hp: 100, maxHp: 100, level: 2, experience: 1500 });
            const xpFill = document.getElementById('xp-fill') as HTMLElement;
            expect(xpFill?.style.width).toBe('50%');

            const xpText = document.getElementById('xp-text');
            expect(xpText?.textContent).toBe('500 / 1000 XP');
        });

        it('limita XP bar a 100% quando XP >= próximo nível', () => {
            ui.update({ hp: 100, maxHp: 100, level: 1, experience: 1500 });
            const xpFill = document.getElementById('xp-fill') as HTMLElement;
            expect(xpFill?.style.width).toBe('100%');
        });
    });

    describe('destroy', () => {
        it('limpa o innerHTML do painel', () => {
            const panel = document.getElementById('character-panel');
            expect(panel?.innerHTML).not.toBe('');

            ui.destroy();

            expect(panel?.innerHTML).toBe('');
        });
    });
});
