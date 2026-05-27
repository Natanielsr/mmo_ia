import type { OverlayConfig } from './types'

export const WEAPON_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'dagger': {
        walkTextureKey: 'weapon_dagger_walk',
        walkAssetPath: 'assets/walkcycle/WEAPON_dagger.png',
        slashTextureKey: 'weapon_dagger_slash',
        slashAssetPath: 'assets/slash/WEAPON_dagger.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}

export function getWeaponOverlayConfig(itemName: string): OverlayConfig | null {
    return WEAPON_OVERLAY_REGISTRY[itemName] ?? null
}
