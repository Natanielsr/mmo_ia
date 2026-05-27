export interface FeetOverlayConfig {
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

export const FEET_OVERLAY_REGISTRY: Record<string, FeetOverlayConfig> = {
    'iron-boots': {
        walkTextureKey: 'feet_iron_boots_walk',
        walkAssetPath: 'assets/walkcycle/FEET_plate_armor_shoes.png',
        slashTextureKey: 'feet_iron_boots_slash',
        slashAssetPath: 'assets/slash/FEET_plate_armor_shoes.png',
        frameWidth: 64,
        frameHeight: 64,
        walkDirectionFrames: { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate: 16,
        slashFrameRate: 24,
    }
}

export function getFeetOverlayConfig(itemName: string): FeetOverlayConfig | null {
    return FEET_OVERLAY_REGISTRY[itemName] ?? null
}
