import { GRID_SIZE } from '../config/constants';
import type { Position } from '../types';

/**
 * Converte posição de grid (servidor) para coordenadas de mundo (Phaser).
 * O eixo Y é invertido: grid crescente para cima → mundo crescente para baixo.
 * offsetX/offsetY permitem ajuste por tipo de entidade (ex: player tem Y_OFFSET=7).
 */
export function gridToWorld(pos: Position, offsetX = 0, offsetY = 0): Position {
    return {
        x: pos.x * GRID_SIZE + GRID_SIZE / 2 - offsetX,
        y: -pos.y * GRID_SIZE - GRID_SIZE / 2 - offsetY,
    };
}
