import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import LoginOverlay from '../../components/LoginOverlay.vue'
import { useGameStore } from '../../stores/gameStore'

function mountWithStore() {
  const pinia = createPinia()
  const wrapper = mount(LoginOverlay, { global: { plugins: [pinia] } })
  const store = useGameStore(pinia)
  return { wrapper, store }
}

describe('LoginOverlay', () => {
  it('visible when not logged in', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.find('#login-overlay').exists()).toBe(true)
    expect(wrapper.find('#login-overlay').isVisible()).toBe(true)
  })

  it('hidden after setLoggedIn', async () => {
    const { wrapper, store } = mountWithStore()
    store.setLoggedIn('Hero')
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#login-overlay').isVisible()).toBe(false)
  })

  it('shows error banner when connectionError is true', async () => {
    const { wrapper, store } = mountWithStore()
    store.setConnectionError(true)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#connection-error').isVisible()).toBe(true)
  })

  it('hides error banner by default', () => {
    const { wrapper } = mountWithStore()
    expect(wrapper.find('#connection-error').isVisible()).toBe(false)
  })

  it('join button disabled when isConnecting', async () => {
    const { wrapper, store } = mountWithStore()
    store.setServerMessage('Connecting...', true)
    await wrapper.vm.$nextTick()
    const btn = wrapper.find('#btn-join')
    expect((btn.element as HTMLButtonElement).disabled).toBe(true)
  })

  it('shows server status when isConnecting', async () => {
    const { wrapper, store } = mountWithStore()
    store.setServerMessage('Conectando...', true)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('#server-status').isVisible()).toBe(true)
    expect(wrapper.find('#status-text').text()).toBe('Conectando...')
  })

  it('calls requestJoin with player name on button click', async () => {
    const { wrapper, store } = mountWithStore()
    const joined: string[] = []
    store.setJoinCallback((name) => joined.push(name))
    const input = wrapper.find<HTMLInputElement>('#username')
    await input.setValue('TestPlayer')
    await wrapper.find('#btn-join').trigger('click')
    expect(joined).toEqual(['TestPlayer'])
  })

  it('does not call requestJoin with empty name', async () => {
    const { wrapper, store } = mountWithStore()
    const joined: string[] = []
    store.setJoinCallback((name) => joined.push(name))
    await wrapper.find('#btn-join').trigger('click')
    expect(joined).toHaveLength(0)
  })
})
