export type DirectionFrames = {
    north: [number, number]
    west:  [number, number]
    south: [number, number]
    east:  [number, number]
}

export interface OverlayConfig {
    walkTextureKey: string
    walkAssetPath: string
    slashTextureKey: string
    slashAssetPath: string
    frameWidth: number
    frameHeight: number
    walkDirectionFrames: DirectionFrames
    slashDirectionFrames: DirectionFrames
    walkFrameRate: number
    slashFrameRate: number
}
