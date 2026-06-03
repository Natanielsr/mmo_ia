/** Percentual de HP clampado entre 0 e 1. */
export function calcHpPercent(hp: number, maxHp: number): number {
    return Math.max(0, hp / (maxHp || 1));
}

/** XP total acumulado necessário para atingir o nível dado. Espelha backend: 200 * level^2.8. */
export function xpThreshold(level: number): number {
    return Math.floor(200 * Math.pow(level, 2.8));
}

/** Percentual de XP dentro do nível atual (0–100). */
export function calcXpPercent(experience: number, level: number): number {
    const prev   = xpThreshold(level - 1);
    const next   = xpThreshold(level);
    const window = next - prev;
    const xpIntoLevel = Math.max(0, experience - prev);
    return Math.min(100, Math.max(0, (xpIntoLevel / window) * 100));
}
