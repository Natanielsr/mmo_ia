import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import CharacterStatusPanel from '../../components/CharacterStatusPanel.vue'
import { useGameStore } from '../../stores/gameStore'

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(CharacterStatusPanel, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('CharacterStatusPanel', () => {
  it('shows player name from store', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Arthas')
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#char-name').text()).toBe('Arthas')
  })

  it('hp bar width reflects hp percentage', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateCharacterStatus({ hp: 50, maxHp: 100 })
    await wrapper.vm.$nextTick()
    const fill = wrapper.find('#hp-fill').element as HTMLElement
    expect(fill.style.width).toBe('50%')
  })

  it('hp bar is 100% when at full hp', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateCharacterStatus({ hp: 100, maxHp: 100 })
    await wrapper.vm.$nextTick()
    const fill = wrapper.find('#hp-fill').element as HTMLElement
    expect(fill.style.width).toBe('100%')
  })

  it('hp bar is 0% when dead', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateCharacterStatus({ hp: 0, maxHp: 100 })
    await wrapper.vm.$nextTick()
    const fill = wrapper.find('#hp-fill').element as HTMLElement
    expect(fill.style.width).toBe('0%')
  })

  it('shows hp text', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateCharacterStatus({ hp: 75, maxHp: 200 })
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#hp-text').text()).toContain('75')
    expect(wrapper.find('#hp-text').text()).toContain('200')
  })

  it('shows level text', async () => {
    const { wrapper, store } = mountWithStore()
    store.updateCharacterStatus({ level: 5 })
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#level-text').text()).toContain('5')
  })

  it('xp bar width reflects xp within current level', async () => {
    const { wrapper, store } = mountWithStore()
    // Level 2: range 1000–2000. At 1500 XP, 50% into level.
    store.updateCharacterStatus({ level: 2, experience: 1500 })
    await wrapper.vm.$nextTick()
    const fill = wrapper.find('#xp-fill').element as HTMLElement
    expect(fill.style.width).toBe('50%')
  })

  it('xp bar is 0% at start of new level', async () => {
    const { wrapper, store } = mountWithStore()
    // Level 1: range 0–1000. At 0 XP, 0%.
    store.updateCharacterStatus({ level: 1, experience: 0 })
    await wrapper.vm.$nextTick()
    const fill = wrapper.find('#xp-fill').element as HTMLElement
    expect(fill.style.width).toBe('0%')
  })
})
