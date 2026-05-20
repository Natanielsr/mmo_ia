import { describe, it, expect } from 'vitest';
import { calcHpPercent, calcXpPercent } from '../../utils/xpCalc';

describe('calcHpPercent', () => {
    it('retorna 1 com HP cheio', () => {
        expect(calcHpPercent(100, 100)).toBe(1);
    });

    it('retorna 0.5 com metade do HP', () => {
        expect(calcHpPercent(50, 100)).toBe(0.5);
    });

    it('retorna 0 com HP zerado', () => {
        expect(calcHpPercent(0, 100)).toBe(0);
    });

    it('clamp: não retorna negativo quando HP < 0', () => {
        expect(calcHpPercent(-10, 100)).toBe(0);
    });

    it('evita divisão por zero quando maxHp é 0', () => {
        expect(calcHpPercent(0, 0)).toBe(0);
    });
});

describe('calcXpPercent', () => {
    it('retorna 0% no início do nível 1 (0 XP)', () => {
        expect(calcXpPercent(0, 1)).toBe(0);
    });

    it('retorna 50% com 500 XP no nível 1', () => {
        expect(calcXpPercent(500, 1)).toBe(50);
    });

    it('retorna 100% com 1000 XP exatos (limite do nível 1)', () => {
        expect(calcXpPercent(1000, 1)).toBe(100);
    });

    it('retorna 0% no início do nível 2 (1000 XP acumulado)', () => {
        expect(calcXpPercent(1000, 2)).toBe(0);
    });

    it('retorna 50% com 1500 XP no nível 2', () => {
        expect(calcXpPercent(1500, 2)).toBe(50);
    });

    it('retorna 100% no limite do nível 2 (2000 XP)', () => {
        expect(calcXpPercent(2000, 2)).toBe(100);
    });

    it('nunca ultrapassa 100%', () => {
        expect(calcXpPercent(9999, 1)).toBe(100);
    });

    it('nunca retorna negativo com XP abaixo do nível', () => {
        // XP 500 mas nivel 2 (prevLevelXp = 1000) → xpIntoLevel = max(0, -500) = 0
        expect(calcXpPercent(500, 2)).toBe(0);
    });

    it('a janela de XP de cada nível é sempre 1000', () => {
        // Para qualquer nível, 500 XP dentro do nível = 50%
        for (const level of [1, 2, 5, 10]) {
            const prevLevelXp = (level - 1) * 1000;
            expect(calcXpPercent(prevLevelXp + 500, level)).toBe(50);
        }
    });
});
