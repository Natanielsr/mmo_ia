import { describe, it, expect } from 'vitest';
import { calcHpPercent, calcXpPercent, xpThreshold } from '../../utils/xpCalc';

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

    it('retorna 50% com 100 XP no nível 1', () => {
        // Nível 1: prev=0, next=200, window=200. Meio = 100
        expect(calcXpPercent(100, 1)).toBe(50);
    });

    it('retorna 100% com 200 XP exatos (limite do nível 1)', () => {
        expect(calcXpPercent(200, 1)).toBe(100);
    });

    it('retorna 0% no início do nível 2 (200 XP acumulado)', () => {
        expect(calcXpPercent(200, 2)).toBe(0);
    });

    it('retorna 50% no meio do nível 2 (796 XP = 200 + 1192/2)', () => {
        // Nível 2: prev=200, next=1392, window=1192. Meio = 200+596=796
        expect(calcXpPercent(796, 2)).toBe(50);
    });

    it('retorna 100% no limite do nível 2 (1392 XP total)', () => {
        expect(calcXpPercent(1392, 2)).toBe(100);
    });

    it('nunca ultrapassa 100%', () => {
        expect(calcXpPercent(9999, 1)).toBe(100);
    });

    it('nunca retorna negativo com XP abaixo do nível', () => {
        // XP 100 mas nivel 2 (prevThreshold = 200) → xpIntoLevel = 0
        expect(calcXpPercent(100, 2)).toBe(0);
    });

    it('cada nível começa em 0% na sua threshold exata', () => {
        // xpThreshold(N) é o XP total onde o jogador está no início do nível N+1
        expect(calcXpPercent(xpThreshold(0), 1)).toBe(0); // início do nível 1
        expect(calcXpPercent(xpThreshold(1), 2)).toBe(0); // início do nível 2
        expect(calcXpPercent(xpThreshold(2), 3)).toBe(0); // início do nível 3
        expect(calcXpPercent(xpThreshold(4), 5)).toBe(0); // início do nível 5
    });
});
