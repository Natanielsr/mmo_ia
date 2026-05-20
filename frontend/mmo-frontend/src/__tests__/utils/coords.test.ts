import { describe, it, expect } from 'vitest';
import { gridToWorld } from '../../utils/coords';

const GRID_SIZE = 64;

describe('gridToWorld', () => {
    it('converte origem (0,0) corretamente', () => {
        const result = gridToWorld({ x: 0, y: 0 });
        expect(result.x).toBe(GRID_SIZE / 2);         // 32
        expect(result.y).toBe(-(GRID_SIZE / 2));       // -32
    });

    it('converte posição positiva no eixo X', () => {
        const result = gridToWorld({ x: 3, y: 0 });
        expect(result.x).toBe(3 * GRID_SIZE + GRID_SIZE / 2); // 224
        expect(result.y).toBe(-(GRID_SIZE / 2));
    });

    it('inverte o eixo Y (grid crescente para cima = mundo negativo)', () => {
        const result = gridToWorld({ x: 0, y: 5 });
        expect(result.x).toBe(GRID_SIZE / 2);
        expect(result.y).toBe(-5 * GRID_SIZE - GRID_SIZE / 2); // -352
    });

    it('aplica offsetX e offsetY quando fornecidos', () => {
        const result = gridToWorld({ x: 1, y: 1 }, 0, 7);
        expect(result.x).toBe(1 * GRID_SIZE + GRID_SIZE / 2);          // 96
        expect(result.y).toBe(-1 * GRID_SIZE - GRID_SIZE / 2 - 7);     // -103
    });

    it('coincide com a fórmula original do PlayerManager (offset Y=7)', () => {
        const gridPos = { x: 4, y: 2 };
        const offsetY = 7;
        const expected = {
            x: gridPos.x * GRID_SIZE + GRID_SIZE / 2,
            y: -gridPos.y * GRID_SIZE - GRID_SIZE / 2 - offsetY,
        };
        expect(gridToWorld(gridPos, 0, offsetY)).toEqual(expected);
    });

    it('coincide com a fórmula original do Monster/HealingPotion (sem offset)', () => {
        const gridPos = { x: 2, y: 3 };
        const expected = {
            x: gridPos.x * GRID_SIZE + GRID_SIZE / 2,
            y: -gridPos.y * GRID_SIZE - GRID_SIZE / 2,
        };
        expect(gridToWorld(gridPos)).toEqual(expected);
    });

    it('funciona com coordenadas negativas', () => {
        const result = gridToWorld({ x: -1, y: -2 });
        expect(result.x).toBe(-1 * GRID_SIZE + GRID_SIZE / 2); // -32
        expect(result.y).toBe(2 * GRID_SIZE - GRID_SIZE / 2);  // 96
    });
});
