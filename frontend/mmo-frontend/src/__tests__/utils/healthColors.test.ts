import { describe, it, expect } from 'vitest';
import { getPlayerHealthColor, getMonsterHealthColor } from '../../utils/healthColors';

describe('getPlayerHealthColor', () => {
    it('retorna verde acima de 50%', () => {
        expect(getPlayerHealthColor(1.0)).toBe(0x00ff00);
        expect(getPlayerHealthColor(0.51)).toBe(0x00ff00);
    });

    it('retorna amarelo entre 25% e 50% (inclusive)', () => {
        expect(getPlayerHealthColor(0.5)).toBe(0xffff00);
        expect(getPlayerHealthColor(0.26)).toBe(0xffff00);
    });

    it('retorna vermelho em 25% ou menos', () => {
        expect(getPlayerHealthColor(0.25)).toBe(0xff0000);
        expect(getPlayerHealthColor(0.1)).toBe(0xff0000);
        expect(getPlayerHealthColor(0)).toBe(0xff0000);
    });
});

describe('getMonsterHealthColor', () => {
    it('retorna verde acima de 60%', () => {
        expect(getMonsterHealthColor(1.0)).toBe(0x00ff00);
        expect(getMonsterHealthColor(0.61)).toBe(0x00ff00);
    });

    it('retorna amarelo entre 30% e 60% (inclusive)', () => {
        expect(getMonsterHealthColor(0.6)).toBe(0xffff00);
        expect(getMonsterHealthColor(0.31)).toBe(0xffff00);
    });

    it('retorna vermelho em 30% ou menos', () => {
        expect(getMonsterHealthColor(0.3)).toBe(0xff0000);
        expect(getMonsterHealthColor(0.1)).toBe(0xff0000);
        expect(getMonsterHealthColor(0)).toBe(0xff0000);
    });

    it('thresholds são diferentes entre Player e Monster', () => {
        // 40% é verde para player mas amarelo para monster
        expect(getPlayerHealthColor(0.40)).toBe(0xffff00); // player: 0.25 < 0.40 ≤ 0.50
        expect(getMonsterHealthColor(0.40)).toBe(0xffff00); // monster: 0.30 < 0.40 ≤ 0.60

        // 55% é verde para player e amarelo para monster
        expect(getPlayerHealthColor(0.55)).toBe(0x00ff00);
        expect(getMonsterHealthColor(0.55)).toBe(0xffff00);
    });
});
