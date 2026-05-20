import { describe, it, expect } from 'vitest';
import { getDirectionAnimation, getIdleFrame } from '../../utils/playerAnimations';

describe('getDirectionAnimation', () => {
    it('retorna string vazia quando não há movimento', () => {
        expect(getDirectionAnimation(0, 0)).toBe('');
    });

    it('retorna walk-east para dx positivo dominante', () => {
        expect(getDirectionAnimation(10, 0)).toBe('walk-east');
        expect(getDirectionAnimation(5, 2)).toBe('walk-east');
    });

    it('retorna walk-west para dx negativo dominante', () => {
        expect(getDirectionAnimation(-10, 0)).toBe('walk-west');
        expect(getDirectionAnimation(-5, 2)).toBe('walk-west');
    });

    it('retorna walk-south para dy positivo dominante', () => {
        expect(getDirectionAnimation(0, 10)).toBe('walk-south');
        expect(getDirectionAnimation(2, 5)).toBe('walk-south');
    });

    it('retorna walk-north para dy negativo dominante', () => {
        expect(getDirectionAnimation(0, -10)).toBe('walk-north');
        expect(getDirectionAnimation(2, -5)).toBe('walk-north');
    });

    it('quando |dx| === |dy|, prefere o eixo Y (walk-south/north)', () => {
        expect(getDirectionAnimation(5, 5)).toBe('walk-south');
        expect(getDirectionAnimation(5, -5)).toBe('walk-north');
    });

    it('funciona com movimento puramente horizontal', () => {
        expect(getDirectionAnimation(1, 0)).toBe('walk-east');
        expect(getDirectionAnimation(-1, 0)).toBe('walk-west');
    });

    it('funciona com movimento puramente vertical', () => {
        expect(getDirectionAnimation(0, 1)).toBe('walk-south');
        expect(getDirectionAnimation(0, -1)).toBe('walk-north');
    });
});

describe('getIdleFrame', () => {
    it('retorna 0 para north', () => {
        expect(getIdleFrame('north')).toBe(0);
        expect(getIdleFrame('walk-north')).toBe(0);
    });

    it('retorna 9 para west', () => {
        expect(getIdleFrame('west')).toBe(9);
        expect(getIdleFrame('walk-west')).toBe(9);
    });

    it('retorna 27 para east', () => {
        expect(getIdleFrame('east')).toBe(27);
        expect(getIdleFrame('walk-east')).toBe(27);
    });

    it('retorna 18 para south (default)', () => {
        expect(getIdleFrame('south')).toBe(18);
        expect(getIdleFrame('walk-south')).toBe(18);
        expect(getIdleFrame('')).toBe(18);
        expect(getIdleFrame('unknown')).toBe(18);
    });
});
