import type { OverlayConfig } from './types'
import { WEAPON_OVERLAY_REGISTRY, getWeaponOverlayConfig } from './weaponOverlayRegistry'
import { ARMOR_OVERLAY_REGISTRY } from './armorOverlayRegistry'
import { LEGS_OVERLAY_REGISTRY } from './legsOverlayRegistry'
import { HEAD_OVERLAY_REGISTRY } from './headOverlayRegistry'
import { FEET_OVERLAY_REGISTRY } from './feetOverlayRegistry'
import { SHIELD_OVERLAY_REGISTRY } from './shieldOverlayRegistry'

export type { OverlayConfig }
export type BodyOverlayConfig = OverlayConfig
export type WeaponOverlayConfig = OverlayConfig
export { getWeaponOverlayConfig, WEAPON_OVERLAY_REGISTRY }

const ALL_SLOT_REGISTRIES: Record<string, Record<string, OverlayConfig>> = {
    Weapon: WEAPON_OVERLAY_REGISTRY,
    Chest:  ARMOR_OVERLAY_REGISTRY,
    Legs:   LEGS_OVERLAY_REGISTRY,
    Helmet: HEAD_OVERLAY_REGISTRY,
    Boots:  FEET_OVERLAY_REGISTRY,
    Shield: SHIELD_OVERLAY_REGISTRY,
}

export const BODY_SLOTS = Object.keys(ALL_SLOT_REGISTRIES).filter(s => s !== 'Weapon')

export function getBodyOverlayConfig(slot: string, itemName: string): OverlayConfig | null {
    return ALL_SLOT_REGISTRIES[slot]?.[itemName] ?? null
}

export function getAllBodyConfigs(): OverlayConfig[] {
    return BODY_SLOTS.flatMap(slot => Object.values(ALL_SLOT_REGISTRIES[slot]))
}
