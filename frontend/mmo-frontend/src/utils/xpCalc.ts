/** Percentual de HP clampado entre 0 e 1. */
export function calcHpPercent(hp: number, maxHp: number): number {
    return Math.max(0, hp / (maxHp || 1));
}

/**
 * Percentual de XP dentro do nível atual (0–100).
 * Fórmula do backend: cada nível requer level * 1000 XP total acumulado.
 * A janela de cada nível é sempre 1000 XP.
 */
export function calcXpPercent(experience: number, level: number): number {
    const prevLevelXp = (level - 1) * 1000;
    const xpIntoLevel = Math.max(0, experience - prevLevelXp);
    return Math.min(100, Math.max(0, (xpIntoLevel / 1000) * 100));
}
