export interface BiomeObjectSpriteConfig {
    asset: string;
    isPassable: boolean;
}

export const BIOME_OBJECT_SPRITE_REGISTRY: Record<string, BiomeObjectSpriteConfig> = {
    // grass biome
    'tree':        { asset: 'assets/tree.png',                      isPassable: false },
    'rock':        { asset: 'assets/rock.png',                      isPassable: false },
    'bush':        { asset: 'assets/bush.png',                      isPassable: false },
    'pillar':      { asset: 'assets/pillar.png',                    isPassable: false },
    'apple_tree':  { asset: 'assets/apple_tree.png',                isPassable: false },

    // dark_forest biome
    'dead_tree':   { asset: 'assets/dark_forest/dead_tree.png',    isPassable: false },
    'dark_tree':   { asset: 'assets/dark_forest/dark_tree.png',     isPassable: false },
    'dark_rock':   { asset: 'assets/dark_forest/dark_rock.png',     isPassable: false },
    'dark_bush':   { asset: 'assets/dark_forest/dark_bush.png',     isPassable: false },
    'dark_pillar': { asset: 'assets/dark_forest/dark_pillar.png',   isPassable: false },

    // sand biome (TODO: add proper sprites)
    'cactus':      { asset: 'assets/cactus.png',                      isPassable: false },
    'cactus2':   { asset: 'assets/cactus2.png',                      isPassable: false },
    'cactus3':    { asset: 'assets/cactus3.png',                      isPassable: false },
    'coconut_palm':    { asset: 'assets/coconut_palm.png',                      isPassable: false },
    'palm':    { asset: 'assets/palm.png',                      isPassable: false },

    // snow biome (TODO: add proper sprites)
    'pine':   { asset: 'assets/pine.png',                      isPassable: false },
    'snow_rock':   { asset: 'assets/rock.png',                      isPassable: false },
    'snow_bush':   { asset: 'assets/bush.png',                      isPassable: false },
};

const FALLBACK: BiomeObjectSpriteConfig = { asset: 'assets/rock.png', isPassable: false };

export function getBiomeObjectConfig(objectCode: string): BiomeObjectSpriteConfig {
    return BIOME_OBJECT_SPRITE_REGISTRY[objectCode] ?? FALLBACK;
}
