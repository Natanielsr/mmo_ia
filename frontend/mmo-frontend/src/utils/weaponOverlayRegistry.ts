export interface WeaponOverlayConfig {
    textureKey: string
    assetPath: string
    frameWidth: number
    frameHeight: number
    directionFrames: {
        north: [number, number]
        west: [number, number]
        south: [number, number]
        east: [number, number]
    }
    frameRate: number
}

export const WEAPON_OVERLAY_REGISTRY: Record<string, WeaponOverlayConfig> = {
    'Dagger': {
        textureKey: 'weapon_dagger',
        assetPath: 'assets/slash/WEAPON_dagger.png',
        frameWidth: 64,
        frameHeight: 64,
        directionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        frameRate: 24,
    }
}

export function getWeaponOverlayConfig(itemName: string): WeaponOverlayConfig | null {
    return WEAPON_OVERLAY_REGISTRY[itemName] ?? null
}
