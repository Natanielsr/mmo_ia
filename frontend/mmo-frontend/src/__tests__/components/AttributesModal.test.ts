import { describe, it, expect, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import AttributesModal from '../../components/AttributesModal.vue'
import { useGameStore } from '../../stores/gameStore'

// Teleport renders into document.body — query from there
function q(sel: string) {
  return document.querySelector<HTMLElement>(sel)
}

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(AttributesModal, {
    attachTo: document.body,
    global: { plugins: [pinia] },
  })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

afterEach(() => {
  document.querySelectorAll('#attributes-modal-overlay').forEach(el => el.remove())
})

describe('AttributesModal', () => {
  it('hidden by default', () => {
    mountWithStore()
    const overlay = q('#attributes-modal-overlay')
    expect(overlay?.style.display).toBe('none')
  })

  it('visible when attributesModalOpen is true', async () => {
    const { store, wrapper } = mountWithStore()
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    const overlay = q('#attributes-modal-overlay')
    expect(overlay?.style.display).not.toBe('none')
  })

  it('shows player name', async () => {
    const { store, wrapper } = mountWithStore()
    store.setLoggedIn('Thrall')
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    expect(q('.attr-row-name .attr-value')?.textContent?.trim()).toBe('Thrall')
  })

  it('shows hp data', async () => {
    const { store, wrapper } = mountWithStore()
    store.updateCharacterStatus({ hp: 80, maxHp: 120 })
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    const hpText = q('.attr-row-hp .attr-value')?.textContent ?? ''
    expect(hpText).toContain('80')
    expect(hpText).toContain('120')
  })

  it('shows level', async () => {
    const { store, wrapper } = mountWithStore()
    store.updateCharacterStatus({ level: 7 })
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    expect(q('.attr-row-level .attr-value')?.textContent?.trim()).toBe('7')
  })

  it('shows attack and defense when available', async () => {
    const { store, wrapper } = mountWithStore()
    store.updateCharacterStatus({ attackPower: 15, defense: 8 })
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    expect(q('.attr-row-attack .attr-value')?.textContent?.trim()).toBe('15')
    expect(q('.attr-row-defense .attr-value')?.textContent?.trim()).toBe('8')
  })

  it('close button hides modal', async () => {
    const { store, wrapper } = mountWithStore()
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    const closeBtn = q('.attributes-modal-close') as HTMLButtonElement
    closeBtn.click()
    await wrapper.vm.$nextTick()
    expect(store.attributesModalOpen).toBe(false)
  })

  it('ESC key closes modal', async () => {
    const { store, wrapper } = mountWithStore()
    store.toggleAttributesModal()
    await wrapper.vm.$nextTick()
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    await wrapper.vm.$nextTick()
    expect(store.attributesModalOpen).toBe(false)
  })
})
