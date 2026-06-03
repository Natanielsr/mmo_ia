import type { OverlayConfig } from './types'

export const FEET_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'plate-boots': {
        walkTextureKey:  'feet_plate_boots_walk',
        walkAssetPath:   'assets/walkcycle/FEET_plate_armor_shoes.png',
        slashTextureKey: 'feet_plate_boots_slash',
        slashAssetPath:  'assets/slash/FEET_plate_armor_shoes.png',
        frameWidth:  64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate:  16,
        slashFrameRate: 24,
        hurtTextureKey: 'feet_plate_boots_hurt',
        hurtAssetPath:  'assets/hurt/FEET_plate_armor_shoes.png',
        hurtFrameRate:  12,
    },
    'leather-shoes': {
        walkTextureKey:  'feet_leather_shoes_walk',
        walkAssetPath:   'assets/walkcycle/FEET_shoes_brown.png',
        slashTextureKey: 'feet_leather_shoes_slash',
        slashAssetPath:  'assets/slash/FEET_shoes_brown.png',
        frameWidth:  64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate:  16,
        slashFrameRate: 24,
        hurtTextureKey: 'feet_leather_shoes_hurt',
        hurtAssetPath:  'assets/hurt/FEET_shoes_brown.png',
        hurtFrameRate:  12,
    }
}
