import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useGameStore } from '../../stores/gameStore'
import type { EquipmentSlot, ItemData } from '../../types'

beforeEach(() => {
  setActivePinia(createPinia())
})

function makeItem(id: string, type: string, slotIndex?: number): ItemData {
  return { id, name: `Item ${id}`, position: { x: 0, y: 0 }, type, slotIndex }
}

describe('setLoggedIn', () => {
  it('sets isLoggedIn and playerName', () => {
    const store = useGameStore()
    store.setLoggedIn('Hero')
    expect(store.isLoggedIn).toBe(true)
    expect(store.playerName).toBe('Hero')
    expect(store.panelBarVisible).toBe(true)
  })
})

describe('updateCharacterStatus', () => {
  it('applies partial updates without touching other fields', () => {
    const store = useGameStore()
    store.updateCharacterStatus({ hp: 50, maxHp: 100 })
    expect(store.hp).toBe(50)
    expect(store.maxHp).toBe(100)
    expect(store.level).toBe(1) // unchanged default
  })

  it('sets attackPower to null when undefined in payload', () => {
    const store = useGameStore()
    store.updateCharacterStatus({ attackPower: undefined })
    expect(store.attackPower).toBeNull()
  })
})

describe('addLog', () => {
  it('adds entry at front (newest first)', () => {
    const store = useGameStore()
    store.addLog('first')
    store.addLog('second')
    expect(store.logEntries[0].message).toBe('second')
    expect(store.logEntries[1].message).toBe('first')
  })

  it('caps at 100 entries', () => {
    const store = useGameStore()
    for (let i = 0; i < 105; i++) store.addLog(`msg ${i}`)
    expect(store.logEntries.length).toBe(100)
  })

  it('marks error entries', () => {
    const store = useGameStore()
    store.addLog('boom', true)
    expect(store.logEntries[0].isError).toBe(true)
  })

  it('clears log', () => {
    const store = useGameStore()
    store.addLog('x')
    store.clearLog()
    expect(store.logEntries.length).toBe(0)
  })
})

describe('updateInventory', () => {
  it('places items at slotIndex positions', () => {
    const store = useGameStore()
    store.updateInventory([makeItem('a', 'Weapon', 3), makeItem('b', 'Armor', 7)])
    expect(store.inventorySlots[3]?.id).toBe('a')
    expect(store.inventorySlots[7]?.id).toBe('b')
    expect(store.inventorySlots[0]).toBeNull()
  })

  it('keeps 20 slots', () => {
    const store = useGameStore()
    store.updateInventory([])
    expect(store.inventorySlots.length).toBe(20)
  })

  it('replaces previous inventory on update', () => {
    const store = useGameStore()
    store.updateInventory([makeItem('a', 'Weapon', 0)])
    store.updateInventory([makeItem('b', 'Armor', 1)])
    expect(store.inventorySlots[0]).toBeNull()
    expect(store.inventorySlots[1]?.id).toBe('b')
  })
})

describe('updateEquipment', () => {
  it('sets slots from record', () => {
    const store = useGameStore()
    const slots: Record<EquipmentSlot, ItemData | null> = {
      Weapon: makeItem('w', 'Weapon'), Helmet: null,
      Chest: null, Legs: null, Boots: null, Shield: null,
    }
    store.updateEquipment(slots)
    expect(store.equipmentSlots.Weapon?.id).toBe('w')
    expect(store.equipmentSlots.Helmet).toBeNull()
  })
})

describe('toggleGearPanel / toggleAttributesModal', () => {
  it('toggles gearPanelOpen', () => {
    const store = useGameStore()
    expect(store.gearPanelOpen).toBe(false)
    store.toggleGearPanel()
    expect(store.gearPanelOpen).toBe(true)
    store.toggleGearPanel()
    expect(store.gearPanelOpen).toBe(false)
  })

  it('toggles attributesModalOpen', () => {
    const store = useGameStore()
    store.toggleAttributesModal()
    expect(store.attributesModalOpen).toBe(true)
  })
})

describe('registerCallbacks / dispatchers', () => {
  it('requestEquip calls registered onEquip', () => {
    const store = useGameStore()
    let called = ''
    store.registerCallbacks({
      onEquip: (id) => { called = id },
      onUnequip: () => {},
      onMoveInInventory: () => {},
      onUseItem: () => {},
    })
    store.requestEquip('item-1')
    expect(called).toBe('item-1')
  })

  it('requestUnequip calls registered onUnequip', () => {
    const store = useGameStore()
    let called: EquipmentSlot | '' = ''
    store.registerCallbacks({
      onEquip: () => {},
      onUnequip: (slot) => { called = slot },
      onMoveInInventory: () => {},
      onUseItem: () => {},
    })
    store.requestUnequip('Weapon')
    expect(called).toBe('Weapon')
  })

  it('dispatchers are no-ops when callbacks not registered', () => {
    const store = useGameStore()
    expect(() => store.requestEquip('x')).not.toThrow()
  })
})
