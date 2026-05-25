import { ARMOR_OVERLAY_REGISTRY, type ArmorOverlayConfig } from './armorOverlayRegistry'
import { LEGS_OVERLAY_REGISTRY } from './legsOverlayRegistry'
import { HEAD_OVERLAY_REGISTRY } from './headOverlayRegistry'
import { FEET_OVERLAY_REGISTRY } from './feetOverlayRegistry'

export type BodyOverlayConfig = ArmorOverlayConfig

const BODY_SLOT_REGISTRIES: Record<string, Record<string, BodyOverlayConfig>> = {
    'Chest':  ARMOR_OVERLAY_REGISTRY,
    'Legs':   LEGS_OVERLAY_REGISTRY,
    'Helmet': HEAD_OVERLAY_REGISTRY,
    'Boots':  FEET_OVERLAY_REGISTRY,
}

export function getBodyOverlayConfig(slot: string, itemName: string): BodyOverlayConfig | null {
    return BODY_SLOT_REGISTRIES[slot]?.[itemName] ?? null
}

export function getAllBodyConfigs(): BodyOverlayConfig[] {
    return Object.values(BODY_SLOT_REGISTRIES).flatMap(r => Object.values(r))
}

export const BODY_SLOTS = Object.keys(BODY_SLOT_REGISTRIES)
