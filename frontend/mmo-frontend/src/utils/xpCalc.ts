/** Percentual de HP clampado entre 0 e 1. */
export function calcHpPercent(hp: number, maxHp: number): number {
    return Math.max(0, hp / (maxHp || 1));
}

/** XP total acumulado necessário para atingir o nível dado. Espelha backend: 10000*(1.1^level - 1). */
export function xpThreshold(level: number): number {
    return Math.floor(10000 * (Math.pow(1.1, level) - 1));
}

/** Percentual de XP dentro do nível atual (0–100). */
export function calcXpPercent(experience: number, level: number): number {
    const prev   = xpThreshold(level - 1);
    const next   = xpThreshold(level);
    const window = next - prev;
    const xpIntoLevel = Math.max(0, experience - prev);
    return Math.min(100, Math.max(0, (xpIntoLevel / window) * 100));
}
