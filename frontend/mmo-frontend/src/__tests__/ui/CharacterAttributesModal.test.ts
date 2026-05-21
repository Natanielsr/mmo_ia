import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { CharacterAttributesModal } from '../../ui/CharacterAttributesModal';

describe('CharacterAttributesModal', () => {
    let modal: CharacterAttributesModal;

    beforeEach(() => {
        document.body.innerHTML = '<div id="attributes-modal-overlay"></div>';
        modal = new CharacterAttributesModal();
    });

    afterEach(() => {
        modal.destroy();
    });

    describe('constructor', () => {
        it('cria elemento #attributes-modal-overlay', () => {
            const overlay = document.getElementById('attributes-modal-overlay');
            expect(overlay).toBeTruthy();
        });

        it('cria a estrutura interna do modal', () => {
            const modalContent = document.querySelector('.attributes-modal-content');
            expect(modalContent).toBeTruthy();
        });

        it('inicializa com display: none', () => {
            const overlay = document.getElementById('attributes-modal-overlay') as HTMLElement;
            expect(overlay.style.display).toBe('none');
        });
    });

    describe('update', () => {
        it('atualiza nome do personagem', () => {
            modal.update({
                name: 'Nataniel',
                level: 1,
                hp: 100,
                maxHp: 100,
                experience: 0,
                attackPower: 10,
                defense: 5
            });
            const name = document.querySelector('.attr-row-name .attr-value');
            expect(name?.textContent).toBe('Nataniel');
        });

        it('atualiza level', () => {
            modal.update({
                name: 'Heroe',
                level: 5,
                hp: 100,
                maxHp: 100,
                experience: 3500,
                attackPower: 15,
                defense: 8
            });
            const level = document.querySelector('.attr-row-level .attr-value');
            expect(level?.textContent).toBe('5');
        });

        it('atualiza HP/MaxHP', () => {
            modal.update({
                name: 'Heroe',
                level: 1,
                hp: 75,
                maxHp: 100,
                experience: 0,
                attackPower: 10,
                defense: 5
            });
            const hp = document.querySelector('.attr-row-hp .attr-value');
            expect(hp?.textContent).toBe('75 / 100');
        });

        it('atualiza XP', () => {
            modal.update({
                name: 'Heroe',
                level: 2,
                hp: 100,
                maxHp: 100,
                experience: 1500,
                attackPower: 10,
                defense: 5
            });
            const xp = document.querySelector('.attr-row-xp .attr-value');
            expect(xp?.textContent).toBe('1500');
        });

        it('atualiza Ataque', () => {
            modal.update({
                name: 'Heroe',
                level: 1,
                hp: 100,
                maxHp: 100,
                experience: 0,
                attackPower: 25,
                defense: 5
            });
            const attack = document.querySelector('.attr-row-attack .attr-value');
            expect(attack?.textContent).toBe('25');
        });

        it('atualiza Defesa', () => {
            modal.update({
                name: 'Heroe',
                level: 1,
                hp: 100,
                maxHp: 100,
                experience: 0,
                attackPower: 10,
                defense: 15
            });
            const defense = document.querySelector('.attr-row-defense .attr-value');
            expect(defense?.textContent).toBe('15');
        });

        it('exibe "-" quando atacPower ausente', () => {
            modal.update({
                name: 'Heroe',
                level: 1,
                hp: 100,
                maxHp: 100,
                experience: 0,
                defense: 5
            });
            const attack = document.querySelector('.attr-row-attack .attr-value');
            expect(attack?.textContent).toBe('-');
        });

        it('exibe "-" quando defense ausente', () => {
            modal.update({
                name: 'Heroe',
                level: 1,
                hp: 100,
                maxHp: 100,
                experience: 0,
                attackPower: 10
            });
            const defense = document.querySelector('.attr-row-defense .attr-value');
            expect(defense?.textContent).toBe('-');
        });
    });

    describe('show/hide', () => {
        it('mostra o modal quando show() é chamado', () => {
            const overlay = document.getElementById('attributes-modal-overlay') as HTMLElement;
            expect(overlay.style.display).toBe('none');

            modal.show();

            expect(overlay.style.display).toBe('flex');
        });

        it('esconde o modal quando hide() é chamado', () => {
            const overlay = document.getElementById('attributes-modal-overlay') as HTMLElement;
            modal.show();
            expect(overlay.style.display).toBe('flex');

            modal.hide();

            expect(overlay.style.display).toBe('none');
        });

        it('alterna visibilidade quando toggle() é chamado', () => {
            const overlay = document.getElementById('attributes-modal-overlay') as HTMLElement;
            expect(overlay.style.display).toBe('none');

            modal.toggle();
            expect(overlay.style.display).toBe('flex');

            modal.toggle();
            expect(overlay.style.display).toBe('none');
        });
    });

    describe('destroy', () => {
        it('limpa o innerHTML da overlay', () => {
            const overlay = document.getElementById('attributes-modal-overlay');
            expect(overlay?.innerHTML).not.toBe('');

            modal.destroy();

            expect(overlay?.innerHTML).toBe('');
        });
    });
});
