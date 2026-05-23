import { describe, it, expect, vi, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import InventoryPanel from '../../components/InventoryPanel.vue'
import { useGameStore } from '../../stores/gameStore'
import type { ItemData } from '../../types'

function makeItem(id: string, slotIndex: number): ItemData {
  return { id, name: `Item ${id}`, position: { x: 0, y: 0 }, type: 'Weapon', slotIndex }
}

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(InventoryPanel, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('InventoryPanel', () => {
  it('renders 20 slots', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.findAll('.inv-slot').length).toBe(20)
  })

  it('shows item name in correct slot', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateInventory([makeItem('a', 3)])
    await wrapper.vm.$nextTick()
    const slots = wrapper.findAll('.inv-slot')
    expect(slots[3].text()).toContain('Item a')
    expect(slots[0].text()).toBe('')
  })

  it('empty slots have no item content', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateInventory([])
    await wrapper.vm.$nextTick()
    const slots = wrapper.findAll('.inv-slot')
    expect(slots[0].text()).toBe('')
  })

  it('slot has data-index attribute', () => {
    const { wrapper } = mountWithStore()
    const slots = wrapper.findAll('.inv-slot')
    expect((slots[5].element as HTMLElement).dataset.index).toBe('5')
  })

  it('filled slot has data-item-id', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateInventory([makeItem('xyz', 2)])
    await wrapper.vm.$nextTick()
    const slot = wrapper.findAll('.inv-slot')[2].element as HTMLElement
    expect(slot.dataset.itemId).toBe('xyz')
  })

  describe('onDragEnd — drop on ground', () => {
    afterEach(() => {
      vi.restoreAllMocks()
    })

    it('calls requestDropItem when dropEffect is none and released over #phaser-game', async () => {
      const { wrapper, store } = mountWithStore()
      store.updateInventory([makeItem('item1', 0)])
      await wrapper.vm.$nextTick()

      const gameArea = document.createElement('div')
      const canvas = document.createElement('canvas')
      gameArea.appendChild(canvas)

      vi.spyOn(document, 'getElementById').mockReturnValue(gameArea)
      document.elementFromPoint = vi.fn().mockReturnValue(canvas)

      const requestDropSpy = vi.spyOn(store, 'requestDropItem')

      const slot = wrapper.findAll('.inv-slot')[0]
      await slot.trigger('dragstart', { dataTransfer: { effectAllowed: '', setData: () => {} } })
      await slot.trigger('dragend', {
        dataTransfer: { dropEffect: 'none' },
        clientX: 100,
        clientY: 100,
      })

      expect(requestDropSpy).toHaveBeenCalledOnce()
    })

    it('does NOT call requestDropItem when dropEffect is none but released over Vue panel', async () => {
      const { wrapper, store } = mountWithStore()
      store.updateInventory([makeItem('item2', 0)])
      await wrapper.vm.$nextTick()

      const gameArea = document.createElement('div')
      const panelElement = document.createElement('div')

      vi.spyOn(document, 'getElementById').mockReturnValue(gameArea)
      document.elementFromPoint = vi.fn().mockReturnValue(panelElement)

      const requestDropSpy = vi.spyOn(store, 'requestDropItem')

      const slot = wrapper.findAll('.inv-slot')[0]
      await slot.trigger('dragstart', { dataTransfer: { effectAllowed: '', setData: () => {} } })
      await slot.trigger('dragend', {
        dataTransfer: { dropEffect: 'none' },
        clientX: 800,
        clientY: 200,
      })

      expect(requestDropSpy).not.toHaveBeenCalled()
    })
  })
})
