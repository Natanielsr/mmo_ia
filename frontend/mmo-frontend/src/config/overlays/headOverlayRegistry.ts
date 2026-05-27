import type { OverlayConfig } from './types'

export const HEAD_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'iron-helmet': {
        walkTextureKey: 'head_iron_helmet_walk',
        walkAssetPath: 'assets/walkcycle/HEAD_plate_armor_helmet.png',
        slashTextureKey: 'head_iron_helmet_slash',
        slashAssetPath: 'assets/slash/HEAD_plate_armor_helmet.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}
