import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import PanelToggleBar from '../../components/PanelToggleBar.vue'
import { useGameStore } from '../../stores/gameStore'

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(PanelToggleBar, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('PanelToggleBar', () => {
  it('hidden when panelBarVisible is false', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.find('#panel-toggle-bar').isVisible()).toBe(false)
  })

  it('visible after setLoggedIn', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Hero')
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#panel-toggle-bar').isVisible()).toBe(true)
  })

  it('gear button toggles gearPanelOpen', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Hero')
    await wrapper.vm.$nextTick()
    expect(store.gearPanelOpen).toBe(false)
    await wrapper.find('#gear-toggle').trigger('click')
    expect(store.gearPanelOpen).toBe(true)
  })

  it('gear button has active class when gear panel open', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Hero')
    store.toggleGearPanel()
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#gear-toggle').classes()).toContain('active')
  })

  it('attributes button toggles attributesModalOpen', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Hero')
    await wrapper.vm.$nextTick()
    await wrapper.find('#attributes-toggle').trigger('click')
    expect(store.attributesModalOpen).toBe(true)
  })
})
