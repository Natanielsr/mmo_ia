/** Retorna cor hex (Phaser) para a barra de HP do jogador. */
export function getPlayerHealthColor(percent: number): number {
    if (percent > 0.5) return 0x00ff00;
    if (percent > 0.25) return 0xffff00;
    return 0xff0000;
}

/** Retorna cor hex (Phaser) para a barra de HP dos monstros. */
export function getMonsterHealthColor(percent: number): number {
    if (percent > 0.6) return 0x00ff00;
    if (percent > 0.3) return 0xffff00;
    return 0xff0000;
}
