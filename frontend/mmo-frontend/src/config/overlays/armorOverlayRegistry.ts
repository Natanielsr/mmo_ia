export interface ArmorOverlayConfig {
    walkTextureKey: string
    walkAssetPath: string
    slashTextureKey: string
    slashAssetPath: string
    frameWidth: number
    frameHeight: number
    walkDirectionFrames: {
        north: [number, number]
        west: [number, number]
        south: [number, number]
        east: [number, number]
    }
    slashDirectionFrames: {
        north: [number, number]
        west: [number, number]
        south: [number, number]
        east: [number, number]
    }
    walkFrameRate: number
    slashFrameRate: number
}

export const ARMOR_OVERLAY_REGISTRY: Record<string, ArmorOverlayConfig> = {
    'leather-vest': {
        walkTextureKey: 'armor_leather_torso_walk',
        walkAssetPath: 'assets/walkcycle/TORSO_leather_armor_torso.png',
        slashTextureKey: 'armor_leather_torso_slash',
        slashAssetPath: 'assets/slash/TORSO_leather_armor_torso.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames: { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}

export function getArmorOverlayConfig(itemName: string): ArmorOverlayConfig | null {
    return ARMOR_OVERLAY_REGISTRY[itemName] ?? null
}
