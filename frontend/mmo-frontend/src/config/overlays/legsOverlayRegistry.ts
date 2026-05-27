import type { OverlayConfig } from './types'

export const LEGS_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'leather-pants': {
        walkTextureKey: 'legs_leather_pants_walk',
        walkAssetPath: 'assets/walkcycle/LEGS_pants_greenish.png',
        slashTextureKey: 'legs_leather_pants_slash',
        slashAssetPath: 'assets/slash/LEGS_pants_greenish.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
        hurtTextureKey: 'legs_leather_pants_hurt',
        hurtAssetPath:  'assets/hurt/LEGS_pants_greenish.png',
        hurtFrameRate:  12,
    }
}
