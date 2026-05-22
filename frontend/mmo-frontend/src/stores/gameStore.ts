import { defineStore } from 'pinia'
import { ref, readonly } from 'vue'
import type { ItemData, EquipmentSlot, PlayerStatusData } from '../types'

export const useGameStore = defineStore('game', () => {
  // ── App phase ────────────────────────────────────────────────────────────
  const isLoggedIn      = ref(false)
  const playerName      = ref('')
  const serverMessage   = ref('')
  const isConnecting    = ref(false)
  const connectionError = ref(false)

  // ── Character status ─────────────────────────────────────────────────────
  const hp          = ref(0)
  const maxHp       = ref(100)
  const level       = ref(1)
  const experience  = ref(0)
  const attackPower = ref<number | null>(null)
  const defense     = ref<number | null>(null)
  const isDead      = ref(false)

  // ── Position display ─────────────────────────────────────────────────────
  const posX = ref(0)
  const posY = ref(0)

  // ── Inventory ────────────────────────────────────────────────────────────
  const inventorySlots = ref<(ItemData | null)[]>(Array(20).fill(null))

  // ── Equipment ────────────────────────────────────────────────────────────
  const equipmentSlots = ref<Record<EquipmentSlot, ItemData | null>>({
    Weapon: null, Helmet: null, Chest: null,
    Legs: null, Boots: null, Shield: null,
  })
  const equipAttackBonus  = ref(0)
  const equipDefenseBonus = ref(0)

  // ── UI panel visibility ───────────────────────────────────────────────────
  const gearPanelOpen       = ref(false)
  const attributesModalOpen = ref(false)
  const panelBarVisible     = ref(false)

  // ── Game log ──────────────────────────────────────────────────────────────
  const logEntries = ref<{ message: string; isError: boolean; id: number }[]>([])
  let logIdCounter = 0

  // ── Callbacks (registered by SignalRService, called by Vue components) ────
  const _onJoinGame            = ref<((name: string) => void) | null>(null)
  const _onEquipItem           = ref<((itemId: string) => void) | null>(null)
  const _onUnequipItem         = ref<((slot: EquipmentSlot) => void) | null>(null)
  const _onMoveItemInInventory = ref<((itemId: string, toIndex: number) => void) | null>(null)
  const _onUseItem             = ref<((itemId: string) => void) | null>(null)
  const _onDropItem            = ref<((itemId: string, x: number, y: number) => void) | null>(null)

  // ── Actions ───────────────────────────────────────────────────────────────
  function setLoggedIn(name: string) {
    playerName.value      = name
    isLoggedIn.value      = true
    panelBarVisible.value = true
  }

  function updateCharacterStatus(data: Partial<PlayerStatusData>) {
    if (data.hp          !== undefined) hp.value          = data.hp
    if (data.maxHp       !== undefined) maxHp.value       = data.maxHp
    if (data.level       !== undefined) level.value       = data.level
    if (data.experience  !== undefined) experience.value  = data.experience
    if (data.attackPower !== undefined) attackPower.value = data.attackPower ?? null
    if (data.defense     !== undefined) defense.value     = data.defense ?? null
    if (data.isDead      !== undefined) isDead.value      = data.isDead
  }

  function setPosition(x: number, y: number) {
    posX.value = x
    posY.value = y
  }

  function updateInventory(items: ItemData[]) {
    const slots: (ItemData | null)[] = Array(20).fill(null)
    for (const item of items) {
      const idx = item.slotIndex ?? 0
      if (idx >= 0 && idx < 20) slots[idx] = item
    }
    inventorySlots.value = slots
  }

  function updateEquipment(slots: Record<EquipmentSlot, ItemData | null>) {
    equipmentSlots.value = { ...slots }
  }

  function setEquipStats(attack: number, def: number) {
    equipAttackBonus.value  = attack
    equipDefenseBonus.value = def
  }

  function addLog(message: string, isError = false) {
    logEntries.value.unshift({ message, isError, id: ++logIdCounter })
    if (logEntries.value.length > 100) {
      logEntries.value = logEntries.value.slice(0, 100)
    }
  }

  function clearLog() {
    logEntries.value = []
  }

  function toggleGearPanel() {
    gearPanelOpen.value = !gearPanelOpen.value
  }

  function openGearPanel() {
    gearPanelOpen.value = true
  }

  function toggleAttributesModal() {
    attributesModalOpen.value = !attributesModalOpen.value
  }

  function setServerMessage(msg: string, connecting: boolean) {
    serverMessage.value = msg
    isConnecting.value  = connecting
  }

  function setConnectionError(val: boolean) {
    connectionError.value = val
  }

  function setJoinCallback(fn: (name: string) => void) {
    _onJoinGame.value = fn
  }

  function requestJoin(name: string) {
    _onJoinGame.value?.(name)
  }

  function registerCallbacks(callbacks: {
    onEquip:           (itemId: string) => void
    onUnequip:         (slot: EquipmentSlot) => void
    onMoveInInventory: (itemId: string, toIndex: number) => void
    onUseItem:         (itemId: string) => void
    onDropItem:        (itemId: string, x: number, y: number) => void
  }) {
    _onEquipItem.value           = callbacks.onEquip
    _onUnequipItem.value         = callbacks.onUnequip
    _onMoveItemInInventory.value = callbacks.onMoveInInventory
    _onUseItem.value             = callbacks.onUseItem
    _onDropItem.value            = callbacks.onDropItem
  }

  function requestEquip(itemId: string) {
    _onEquipItem.value?.(itemId)
  }

  function requestUnequip(slot: EquipmentSlot) {
    _onUnequipItem.value?.(slot)
  }

  function requestMoveInInventory(itemId: string, toIndex: number) {
    _onMoveItemInInventory.value?.(itemId, toIndex)
  }

  function requestUseItem(itemId: string) {
    _onUseItem.value?.(itemId)
  }

  function requestDropItem(itemId: string, x: number, y: number) {
    _onDropItem.value?.(itemId, x, y)
  }

  return {
    // state
    isLoggedIn:    readonly(isLoggedIn),
    playerName:    readonly(playerName),
    serverMessage: readonly(serverMessage),
    isConnecting:  readonly(isConnecting),
    connectionError: readonly(connectionError),
    hp, maxHp, level, experience, attackPower, defense, isDead,
    posX, posY,
    inventorySlots: readonly(inventorySlots),
    equipmentSlots: readonly(equipmentSlots),
    equipAttackBonus:  readonly(equipAttackBonus),
    equipDefenseBonus: readonly(equipDefenseBonus),
    gearPanelOpen, attributesModalOpen, panelBarVisible,
    logEntries: readonly(logEntries),
    // actions
    setLoggedIn, updateCharacterStatus, setPosition,
    setJoinCallback, requestJoin,
    updateInventory, updateEquipment, setEquipStats,
    addLog, clearLog,
    toggleGearPanel, openGearPanel, toggleAttributesModal,
    setServerMessage, setConnectionError,
    registerCallbacks,
    requestEquip, requestUnequip, requestMoveInInventory, requestUseItem, requestDropItem,
  }
})
