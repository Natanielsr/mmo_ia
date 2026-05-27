import type { OverlayConfig } from './types'

export const FEET_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'iron-boots': {
        walkTextureKey: 'feet_iron_boots_walk',
        walkAssetPath: 'assets/walkcycle/FEET_plate_armor_shoes.png',
        slashTextureKey: 'feet_iron_boots_slash',
        slashAssetPath: 'assets/slash/FEET_plate_armor_shoes.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}
