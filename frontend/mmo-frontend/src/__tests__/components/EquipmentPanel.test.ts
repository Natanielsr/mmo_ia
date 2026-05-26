import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import EquipmentPanel from '../../components/EquipmentPanel.vue'
import { useGameStore } from '../../stores/gameStore'
import type { ItemData, EquipmentSlot } from '../../types'

function makeItem(id: string, type: string): ItemData {
  return { id, name: `Item ${id}`, position: { x: 0, y: 0 }, type }
}

function allEmpty(): Record<EquipmentSlot, ItemData | null> {
  return { Weapon: null, Helmet: null, Chest: null, Legs: null, Boots: null, Shield: null }
}

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(EquipmentPanel, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('EquipmentPanel', () => {
  it('renders 6 equipment slots', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.findAll('.eq-slot').length).toBe(6)
  })

  it('each slot has data-slot attribute', () => {
    const { wrapper } = mountWithStore()
    const slots = wrapper.findAll('.eq-slot')
    const names = slots.map(s => (s.element as HTMLElement).dataset.slot)
    expect(names).toContain('Weapon')
    expect(names).toContain('Helmet')
    expect(names).toContain('Shield')
  })

  it('shows item name in equipped slot', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateEquipment({ ...allEmpty(), Weapon: makeItem('sword', 'Weapon') })
    await wrapper.vm.$nextTick()
    const weaponSlot = wrapper.find('[data-slot="Weapon"]').element as HTMLElement
    expect(weaponSlot.dataset.itemId).toBe('sword')
  })

  it('empty slot shows no item', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateEquipment(allEmpty())
    await wrapper.vm.$nextTick()
    const weaponSlot = wrapper.find('[data-slot="Weapon"]')
    expect(weaponSlot.text()).toBe('')
  })

  it('filled slot has data-item-id', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateEquipment({ ...allEmpty(), Helmet: makeItem('helm1', 'Helmet') })
    await wrapper.vm.$nextTick()
    const helmetSlot = wrapper.find('[data-slot="Helmet"]').element as HTMLElement
    expect(helmetSlot.dataset.itemId).toBe('helm1')
  })

  it('calls requestUnequip when clicking filled slot', async () => {
    const { wrapper, store } = mountWithStore()
    const unequipped: string[] = []
    store.registerCallbacks({
      onEquip: () => {},
      onUnequip: (slot) => unequipped.push(slot),
      onMoveInInventory: () => {},
      onUseItem: () => {},
      onDropItem: () => {},
    })
    store.updateEquipment({ ...allEmpty(), Weapon: makeItem('w1', 'Weapon') })
    await wrapper.vm.$nextTick()
    await wrapper.find('[data-slot="Weapon"]').trigger('click')
    expect(unequipped).toEqual(['Weapon'])
  })
})
