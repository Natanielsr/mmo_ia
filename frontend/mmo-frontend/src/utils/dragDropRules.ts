import type { ItemData, EquipmentSlot } from '../types'

const SLOT_TYPE_MAP: Record<EquipmentSlot, string> = {
  Weapon: 'Weapon',
  Helmet: 'Helmet',
  Chest:  'Armor',
  Legs:   'Legs',
  Boots:  'Boots',
  Shield: 'Shield',
}

const EQUIPPABLE_TYPES = new Set(Object.values(SLOT_TYPE_MAP))

export function isEquippable(itemType: string): boolean {
  return EQUIPPABLE_TYPES.has(itemType)
}

export function canDropOnSlot(item: ItemData, slot: EquipmentSlot): boolean {
  return item.type === SLOT_TYPE_MAP[slot]
}
