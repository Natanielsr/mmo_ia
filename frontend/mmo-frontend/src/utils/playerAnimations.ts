export function getDirectionAnimation(dx: number, dy: number): string {
    if (dx === 0 && dy === 0) return '';
    if (Math.abs(dx) > Math.abs(dy)) return dx > 0 ? 'walk-east' : 'walk-west';
    return dy > 0 ? 'walk-south' : 'walk-north';
}

export function getIdleFrame(direction: string): number {
    if (direction.includes('north')) return 0;
    if (direction.includes('west')) return 9;
    if (direction.includes('east')) return 27;
    return 18; // south (default)
}
