import type { OverlayConfig } from './types'

export const SHIELD_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'wooden-shield': {
        walkTextureKey: 'shield_wooden_walk',
        walkAssetPath: 'assets/walkcycle/WEAPON_shield_cutout_body.png',
        slashTextureKey: 'shield_wooden_slash',
        slashAssetPath: 'assets/slash/WEAPON_shield_cutout_body.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        walkFrameRate: 16,
        slashFrameRate: 16,
    }
}
