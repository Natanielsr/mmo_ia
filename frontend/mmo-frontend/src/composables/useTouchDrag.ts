import { dragState } from '../utils/dragState'
import { canDropOnSlot } from '../utils/dragDropRules'
import { useGameStore } from '../stores/gameStore'
import type { EquipmentSlot, ItemData } from '../types'

// Module-level so ghost persists across component instances during a single drag
let touchGhost: HTMLElement | null = null
let touchSrcEl: HTMLElement | null = null

export function useTouchDrag() {
  const store = useGameStore()

  function startTouchDrag(
    e: TouchEvent,
    item: ItemData,
    source: 'inventory' | 'equipment',
    sourceSlot = '',
  ) {
    const touch = e.touches[0]
    dragState.itemId = item.id
    dragState.itemType = item.type
    dragState.source = source
    dragState.sourceSlot = sourceSlot

    touchSrcEl = e.currentTarget as HTMLElement
    touchSrcEl.classList.add('dragging')

    touchGhost = touchSrcEl.cloneNode(true) as HTMLElement
    const w = touchSrcEl.offsetWidth
    const h = touchSrcEl.offsetHeight
    touchGhost.style.cssText = `
      position:fixed;pointer-events:none;opacity:0.7;z-index:9999;
      width:${w}px;height:${h}px;
      left:${touch.clientX - w / 2}px;top:${touch.clientY - h / 2}px;
    `
    document.body.appendChild(touchGhost)
  }

  function moveTouchDrag(e: TouchEvent) {
    if (!touchGhost || !dragState.itemId) return
    e.preventDefault()
    const touch = e.touches[0]
    touchGhost.style.left = `${touch.clientX - touchGhost.offsetWidth / 2}px`
    touchGhost.style.top = `${touch.clientY - touchGhost.offsetHeight / 2}px`
  }

  function endTouchDrag(e: TouchEvent) {
    const touch = e.changedTouches[0]

    if (touchGhost) { touchGhost.remove(); touchGhost = null }
    if (touchSrcEl) { touchSrcEl.classList.remove('dragging'); touchSrcEl = null }

    if (!dragState.itemId) return

    const el = document.elementFromPoint(touch.clientX, touch.clientY) as HTMLElement | null
    const invTarget = el?.closest('[data-index]') as HTMLElement | null
    const eqTarget = el?.closest('[data-slot]') as HTMLElement | null

    if (invTarget) {
      const toIndex = parseInt(invTarget.dataset.index!)
      if (dragState.source === 'equipment') {
        store.requestUnequip(dragState.sourceSlot as EquipmentSlot)
      } else {
        store.requestMoveInInventory(dragState.itemId, toIndex)
      }
    } else if (eqTarget) {
      const slotName = eqTarget.dataset.slot as EquipmentSlot
      const fakeItem: ItemData = {
        id: dragState.itemId,
        name: '',
        position: { x: 0, y: 0 },
        type: dragState.itemType,
      }
      if (canDropOnSlot(fakeItem, slotName)) {
        store.requestEquip(dragState.itemId)
      }
    }

    dragState.itemId = ''
    dragState.itemType = ''
    dragState.source = 'inventory'
    dragState.sourceSlot = ''
  }

  return { startTouchDrag, moveTouchDrag, endTouchDrag }
}
