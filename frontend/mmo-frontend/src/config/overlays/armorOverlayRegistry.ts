import type { OverlayConfig } from './types'

export const ARMOR_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'leather-vest': {
        walkTextureKey: 'armor_leather_torso_walk',
        walkAssetPath: 'assets/walkcycle/TORSO_leather_armor_torso.png',
        slashTextureKey: 'armor_leather_torso_slash',
        slashAssetPath: 'assets/slash/TORSO_leather_armor_torso.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
        hurtTextureKey: 'armor_leather_torso_hurt',
        hurtAssetPath:  'assets/hurt/TORSO_leather_armor_torso.png',
        hurtFrameRate:  12,
    }
}
