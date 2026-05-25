export interface LegsOverlayConfig {
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

export const LEGS_OVERLAY_REGISTRY: Record<string, LegsOverlayConfig> = {
    'Leather Pants': {
        walkTextureKey: 'legs_leather_pants_walk',
        walkAssetPath: 'assets/walkcycle/LEGS_pants_greenish.png',
        slashTextureKey: 'legs_leather_pants_slash',
        slashAssetPath: 'assets/slash/LEGS_pants_greenish.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames: { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}

export function getLegsOverlayConfig(itemName: string): LegsOverlayConfig | null {
    return LEGS_OVERLAY_REGISTRY[itemName] ?? null
}
