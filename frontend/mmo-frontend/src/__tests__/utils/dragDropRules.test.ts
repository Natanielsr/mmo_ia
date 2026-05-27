import { describe, it, expect } from 'vitest'
import { canDropOnSlot, isEquippable } from '../../utils/dragDropRules'
import type { ItemData, EquipmentSlot } from '../../types'

function makeItem(type: string): ItemData {
  return { id: '1', name: 'test', tagName: '', position: { x: 0, y: 0 }, type }
}

describe('canDropOnSlot', () => {
  const cases: [string, EquipmentSlot, boolean][] = [
    ['Weapon', 'Weapon', true],
    ['Armor',  'Weapon', false],
    ['Helmet', 'Helmet', true],
    ['Armor',  'Helmet', false],
    ['Armor',  'Chest',  true],
    ['Weapon', 'Chest',  false],
    ['Legs',   'Legs',   true],
    ['Armor',  'Legs',   false],
    ['Boots',  'Boots',  true],
    ['Armor',  'Boots',  false],
    ['Shield', 'Shield', true],
    ['Weapon', 'Shield', false],
  ]

  for (const [itemType, slot, expected] of cases) {
    it(`${itemType} → ${slot} = ${expected}`, () => {
      expect(canDropOnSlot(makeItem(itemType), slot)).toBe(expected)
    })
  }
})

describe('isEquippable', () => {
  for (const type of ['Weapon', 'Armor', 'Helmet', 'Shield', 'Legs', 'Boots']) {
    it(`${type} → true`, () => expect(isEquippable(type)).toBe(true))
  }
  for (const type of ['Potion', '', 'Unknown']) {
    it(`${type} → false`, () => expect(isEquippable(type)).toBe(false))
  }
})
