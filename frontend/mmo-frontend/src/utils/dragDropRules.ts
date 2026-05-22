import type { ItemData, EquipmentSlot } from '../types'

const SLOT_TYPE_MAP: Record<EquipmentSlot, string> = {
  Weapon: 'Weapon',
  Helmet: 'Helmet',
  Chest:  'Armor',
  Legs:   'Legs',
  Boots:  'Boots',
  Shield: 'Shield',
}

export function canDropOnSlot(item: ItemData, slot: EquipmentSlot): boolean {
  return item.type === SLOT_TYPE_MAP[slot]
}
