import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import GameLogPanel from '../../components/GameLogPanel.vue'
import { useGameStore } from '../../stores/gameStore'

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(GameLogPanel, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('GameLogPanel', () => {
  it('renders empty initially', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.findAll('.log-entry').length).toBe(0)
  })

  it('shows log entry after store.addLog', async () => {
    const { wrapper, store } = mountWithStore()
    store.addLog('hello world')
    await wrapper.vm.$nextTick()
    const entries = wrapper.findAll('.log-entry')
    expect(entries.length).toBe(1)
    expect(entries[0].text()).toContain('hello world')
  })

  it('newest entry appears first', async () => {
    const { wrapper, store } = mountWithStore()
    store.addLog('first')
    store.addLog('second')
    await wrapper.vm.$nextTick()
    const entries = wrapper.findAll('.log-entry')
    expect(entries[0].text()).toContain('second')
    expect(entries[1].text()).toContain('first')
  })

  it('error entry has error class', async () => {
    const { wrapper, store } = mountWithStore()
    store.addLog('oops', true)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.log-entry').classes()).toContain('error')
  })

  it('shows multiple entries', async () => {
    const { wrapper, store } = mountWithStore()
    store.addLog('msg 1')
    store.addLog('msg 2')
    store.addLog('msg 3')
    await wrapper.vm.$nextTick()
    expect(wrapper.findAll('.log-entry').length).toBe(3)
  })
})
