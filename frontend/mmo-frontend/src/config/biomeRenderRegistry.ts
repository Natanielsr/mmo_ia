export interface BiomeRenderConfig {
    groundTilePrefix: string;
    groundTileCount: number;
}

export const BIOME_RENDER_REGISTRY: Record<string, BiomeRenderConfig> = {
    green_field: { groundTilePrefix: 'grass',     groundTileCount: 12 },
    dark_forest: { groundTilePrefix: 'dark_grass', groundTileCount: 12 },
};

export function getBiomeRenderConfig(biomeTag: string): BiomeRenderConfig {
    return BIOME_RENDER_REGISTRY[biomeTag] ?? BIOME_RENDER_REGISTRY['green_field'];
}
