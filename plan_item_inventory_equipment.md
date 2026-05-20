# Plano de Implementação — Item Drop, Inventário e Equipamentos

> **Abordagem obrigatória: TDD (Test-Driven Development)**
> Red → Green → Refactor em toda a implementação.
> Testes escritos ANTES do código em cada fase, tanto backend (xUnit) quanto frontend (Vitest).

---

## Estado Atual (o que já existe)

### Backend
- `ItemType` enum: `Generic`, `Potion`
- Classe abstrata `Item`: `Id`, `Name`, `Position`, `Weight`, `ItemType`
- `HealingPotion : Item` — drop em morte de monstro (30% chance), cura automática ao pisar
- `IItemManager` com `DropItem`, `GetItemAt`, `RemoveItem`, `GetAllItems`, `GetItemsInChunk`
- `InventoryService` stub — **NÃO integrado ao Player**
- `IPlayer.EquipItem(string)` e `IPlayer.UnequipItem(string)` — **stubs vazios**
- `ItemData` DTO: `Id`, `Name`, `Position`, `Type`
- Eventos SignalR funcionando: `ItemDropped`, `ItemPickedUp`, `SyncItems`
- `ChunkData.Items` inclui itens nos chunks carregados

### Frontend
- Entidade `HealingPotion` (Phaser sprite com animação de flutuação)
- `ItemManager` (frontend) — `Map<string, HealingPotion>`, apenas potions
- Interface `ItemData`: `id`, `name`, `position`, `type`
- Listeners SignalR para `SyncItems`, `ItemDropped`, `ItemPickedUp`

---

## Dependências entre Fases

```
Fase 1 → Fase 2 → Fase 3 → Fase 4
(tipos)   (inv)    (equip)   (UI)
```

Dentro de cada fase, testes de backend e frontend podem ser escritos em paralelo.
A integração SignalR só pode ser testada após o backend estar pronto.

---

## Fase 1 — Expansão de Tipos de Item e Tabela de Loot

### O que construir
Adicionar `Weapon` e `Armor` como tipos de item com stats. Substituir o drop hardcoded de poção por um `LootTableService` configurável e testável.

### Testes a escrever primeiro

**Backend — `GameServer.Tests/Items/WeaponTests.cs`**
- `Weapon_Should_Have_ItemType_Weapon`
- `Weapon_Should_Expose_AttackBonus`
- `Weapon_With_Zero_AttackBonus_Is_Valid`
- `Armor_Should_Have_ItemType_Armor`
- `Armor_Should_Expose_DefenseBonus`

**Backend — `GameServer.Tests/Items/LootTableServiceTests.cs`**
- `RollLoot_Returns_Null_When_RollFails`
- `RollLoot_Returns_HealingPotion_Within_Chance`
- `RollLoot_Returns_Weapon_Within_Chance`
- `RollLoot_Returns_Armor_Within_Chance`
- `RollLoot_Uses_Provided_Rng_For_Determinism`
- `RollLoot_Never_Returns_Item_When_Chance_Is_Zero`
- `RollLoot_Always_Returns_Item_When_Chance_Is_One`

**Backend — `GameServer.Tests/Managers/MonsterDeathDropTests.cs`**
- `Monster_Death_Can_Drop_Weapon`
- `Monster_Death_Can_Drop_Armor`
- `Monster_Death_Drop_Uses_LootTable_Not_Hardcoded_Logic`
- `ItemDropped_Event_Includes_Stats_In_Payload`

**Frontend — `src/__tests__/managers/ItemManager.test.ts`**
- `syncItem — cria HealingPotion para tipo Potion`
- `syncItem — cria WorldWeapon para tipo Weapon`
- `syncItem — cria WorldArmor para tipo Armor`
- `syncItem — ignora tipo desconhecido sem lançar erro`
- `removeItem — chama collect() e remove do mapa`
- `clear — destrói todos os itens`

### Implementação (após os testes falharem)

**Backend:**
1. Adicionar `Weapon` e `Armor` ao enum `ItemType` em `IItem.cs`
2. Criar `World/Weapon.cs` com propriedade `AttackBonus`
3. Criar `World/Armor.cs` com propriedade `DefenseBonus`
4. Adicionar `int? AttackBonus` e `int? DefenseBonus` ao `ItemData` DTO
5. Criar interface `ILootTableService`:
   ```csharp
   IItem? RollLoot(Position position, string itemId, Func<double>? rng = null);
   ```
6. Criar `Services/LootTableService.cs` — tabela configurável (30% poção, 10% arma, 10% armadura)
7. Substituir bloco hardcoded em `WorldManager.ProcessPlayerAttackMonster` por `_lootTableService.RollLoot(...)`
8. Registrar `ILootTableService` como singleton em `Program.cs`
9. Atualizar `SignalREventEmitter.OnItemDropped` para mapear `AttackBonus`/`DefenseBonus`

**Frontend:**
1. Adicionar `attackBonus?: number` e `defenseBonus?: number` à interface `ItemData` em `types.ts`
2. Criar `entities/WorldWeapon.ts` (sprite com animação de flutuação, textura `'sword'`)
3. Criar `entities/WorldArmor.ts` (mesma estrutura, textura `'armor'`)
4. Criar interface `entities/IWorldItem.ts`:
   ```typescript
   export interface IWorldItem {
       id: string;
       collect(): void;
       destroy(): void;
   }
   ```
5. Ampliar `ItemManager` de `Map<string, HealingPotion>` para `Map<string, IWorldItem>` com factory dispatch em `syncItem`

### Invariante crítico
O drop de `HealingPotion` na morte do monstro não pode quebrar — `LootTableService` deve incluí-la na tabela.

---

## Fase 2 — Sistema de Inventário (Backend + Frontend)

### O que construir
`PlayerInventory` (20 slots, por jogador), `IInventoryManager`, pickup passa para o inventário em vez de cura automática. `UseItem`/`DropItem` acionados via SignalR.

### Testes a escrever primeiro

**Backend — `GameServer.Tests/Inventory/PlayerInventoryTests.cs`**
- `AddItem_Returns_True_And_Item_Is_In_Inventory`
- `AddItem_Returns_False_When_Inventory_Is_Full_At_20_Slots`
- `AddItem_Returns_False_For_Null_Item`
- `RemoveItem_Returns_True_And_Item_Is_Gone`
- `RemoveItem_Returns_False_When_Item_Not_Found`
- `UseItem_Returns_False_When_Item_Not_Found`
- `UseItem_On_Potion_Heals_Player`
- `UseItem_On_Non_Potion_Returns_False`
- `DropItem_Removes_Item_From_Inventory_And_Sets_Position`
- `DropItem_Returns_False_When_Item_Not_Found`
- `GetItems_Returns_Correct_Count`
- `Inventory_Starts_Empty`

**Backend — `GameServer.Tests/Managers/InventoryManagerTests.cs`**
- `GetOrCreate_Creates_Inventory_For_New_Player`
- `GetOrCreate_Returns_Existing_Inventory_For_Same_Player`
- `GetInventory_Returns_Null_For_Unknown_Player`
- `RemovePlayerInventory_Removes_Entry`
- `AddItem_To_Player_Delegates_To_PlayerInventory`

**Backend — `GameServer.Tests/Managers/WorldManagerPickupTests.cs`**
- `Pickup_Does_Not_Auto_Heal_Player`
- `Pickup_Adds_Item_To_Player_Inventory`
- `Pickup_Fails_Silently_When_Inventory_Full`
- `Pickup_Fires_InventoryUpdated_Event`
- `UseItem_From_Inventory_Heals_Player`
- `DropItem_From_Inventory_Places_Item_On_Map`

**Frontend — `src/__tests__/managers/InventoryManager.test.ts`**
- `addItem — adiciona item ao array de slots`
- `addItem — não ultrapassa 20 itens`
- `removeItem — remove por itemId`
- `getItems — retorna snapshot atual`
- `useItem — dispara callback onUseItem com itemId`
- `dropItem — dispara callback onDropItem com itemId`
- `clear — esvazia todos os itens`
- `onInventoryUpdated — substitui inventário inteiro pelo payload do servidor`

### Implementação (após os testes falharem)

**Backend:**
1. Criar `World/PlayerInventory.cs`:
   ```csharp
   public class PlayerInventory(int maxSlots = 20)
   {
       private readonly List<IItem> _items = new();
       public bool AddItem(IItem item) { ... }
       public bool RemoveItem(string itemId) { ... }
       public bool UseItem(string itemId, IPlayer player) { ... }
       public bool DropItem(string itemId, Position pos) { ... }
       public IReadOnlyList<IItem> GetItems() => _items.AsReadOnly();
   }
   ```
2. Criar interface `IInventoryManager`:
   ```csharp
   public interface IInventoryManager
   {
       PlayerInventory GetOrCreate(long playerId);
       PlayerInventory? GetInventory(long playerId);
       void Remove(long playerId);
       bool AddItem(long playerId, IItem item);
   }
   ```
3. Criar `Managers/InventoryManager.cs` com `ConcurrentDictionary<long, PlayerInventory>`
4. Registrar `IInventoryManager` como singleton em `Program.cs`
5. Modificar `WorldManager.ProcessItemPickup`: remover a cura automática, chamar `_inventoryManager.AddItem(player.Id, item)`, disparar `OnInventoryUpdated`
6. Adicionar `OnInventoryUpdated(long playerId, IReadOnlyList<IItem> items)` em `IWorldEvents`
7. Adicionar e implementar em `WorldManager`:
   - `ProcessUseItem(IPlayer player, string itemId)`
   - `ProcessDropItem(IPlayer player, string itemId, Position targetPos)`
8. Implementar `OnInventoryUpdated` em `SignalREventEmitter` — envia para conexão específica do jogador
9. Adicionar métodos ao `GameHub`:
   - `RequestPickupItem(string itemId)`
   - `RequestUseItem(string itemId)`
   - `RequestDropItem(string itemId, int x, int y)`
10. Em `OnDisconnectedAsync` do `GameHub`, chamar `_inventoryManager.Remove(player.Id)`

**Frontend:**
1. Criar `managers/InventoryManager.ts` (sem dependência do Phaser — estado puro):
   ```typescript
   export class InventoryManager {
       private items: ItemData[] = [];
       onUseItem?: (itemId: string) => void;
       onDropItem?: (itemId: string, x: number, y: number) => void;
       addItem(item: ItemData): void;
       removeItem(itemId: string): void;
       getItems(): ItemData[];
       useItem(itemId: string): void;
       dropItem(itemId: string, x: number, y: number): void;
       onInventoryUpdated(items: ItemData[]): void;
       clear(): void;
   }
   ```
2. Instanciar em `MainScene.create()`, wiring `onUseItem`/`onDropItem` para `signalR.invoke(...)`
3. Adicionar listener `"InventoryUpdated"` em `SignalRService`

### Invariante crítico
O antigo `InventoryService` stub deve permanecer registrado no DI durante a transição para não quebrar os test files `InventorySystemTests.cs` e `InventoryImplTests.cs`.

### Novos eventos SignalR

| Evento | Direção | Payload |
|--------|---------|---------|
| `InventoryUpdated` | Servidor → cliente específico | `ItemData[]` |
| `RequestPickupItem` | Cliente → Servidor | `itemId: string` |
| `RequestUseItem` | Cliente → Servidor | `itemId: string` |
| `RequestDropItem` | Cliente → Servidor | `itemId: string, x: int, y: int` |

---

## Fase 3 — Sistema de Equipamentos (Backend + Frontend)

### O que construir
`EquipmentSlot` enum (Weapon/Helmet/Chest/Legs/Boots/Shield), `PlayerEquipment`, `IEquipmentManager`. `TotalAttackPower`/`TotalDefense` no `Player` com redução de dano pelo Defense. `PlayerStatusData` ganha `AttackPower` e `Defense`.

### Testes a escrever primeiro

**Backend — `GameServer.Tests/World/PlayerEquipmentTests.cs`**
- `Player_BaseAttackPower_Equals_AttackPoints_Before_Equipment`
- `Player_BaseDefense_Is_Zero_Before_Equipment`
- `EquipWeapon_Increases_TotalAttackPower`
- `EquipWeapon_When_SlotOccupied_Replaces_And_Returns_Old_Item`
- `UnequipWeapon_Reduces_TotalAttackPower_Back_To_Base`
- `EquipArmor_Increases_TotalDefense`
- `UnequipArmor_Reduces_TotalDefense`
- `EquipItem_DeadPlayer_DoesNothing`
- `GetEquippedItem_Returns_Null_When_Slot_Empty`
- `GetEquippedItem_Returns_Item_After_Equip`
- `TakeDamage_Is_Reduced_By_Defense`

**Backend — `GameServer.Tests/Managers/EquipmentManagerTests.cs`**
- `GetOrCreate_Creates_Equipment_For_New_Player`
- `GetEquipment_Returns_Null_For_Unknown_Player`
- `EquipItem_Delegates_To_PlayerEquipment`
- `UnequipItem_Delegates_To_PlayerEquipment`
- `Remove_Cleans_Up_Entry`

**Backend — `GameServer.Tests/Managers/WorldManagerEquipmentTests.cs`**
- `ProcessEquipItem_Equips_From_Inventory_And_Fires_Event`
- `ProcessEquipItem_Returns_False_When_Item_Not_In_Inventory`
- `ProcessEquipItem_Returns_False_When_Item_Not_Equippable`
- `ProcessUnequipItem_Returns_Item_To_Inventory`
- `ProcessUnequipItem_Returns_False_When_Nothing_Equipped_In_Slot`
- `EquipWeapon_Updates_PlayerStatusData_AttackPower`

**Frontend — `src/__tests__/managers/EquipmentManager.test.ts`**
- `equipItem — armazena item no slot correto`
- `equipItem — substituir slot retorna item antigo`
- `unequipItem — remove item do slot`
- `getSlots — retorna todos os 6 slots`
- `getEquippedItem — retorna null para slot vazio`
- `onEquipmentUpdated — substitui estado completo dos equipamentos`
- `getTotalAttackBonus — soma bônus das armas equipadas`
- `getTotalDefenseBonus — soma bônus das armaduras equipadas`

### Implementação (após os testes falharem)

**Backend:**
1. Criar `Contracts/Types/EquipmentSlot.cs`:
   ```csharp
   public enum EquipmentSlot { Weapon, Helmet, Chest, Legs, Boots, Shield }
   ```
2. Adicionar `EquipmentSlot? Slot { get; }` à interface `IItem` (nullable — só itens equipáveis retornam valor). Override em `Weapon` (`EquipmentSlot.Weapon`) e subclasses de `Armor`
3. Adicionar à `IPlayer`:
   ```csharp
   int TotalAttackPower { get; }
   int TotalDefense { get; }
   IItem? GetEquippedItem(EquipmentSlot slot);
   IItem? EquipFromInventory(PlayerInventory inventory, string itemId);
   IItem? UnequipToInventory(PlayerInventory inventory, EquipmentSlot slot);
   ```
4. Implementar em `Player.cs`. Modificar `TakeDamage`: `damage = Math.Max(0, damage - TotalDefense)`
5. Criar interface `IEquipmentManager`:
   ```csharp
   public interface IEquipmentManager
   {
       PlayerEquipment GetOrCreate(long playerId);
       PlayerEquipment? GetEquipment(long playerId);
       void Remove(long playerId);
   }
   ```
6. Criar `World/PlayerEquipment.cs` com `Dictionary<EquipmentSlot, IItem>`
7. Criar `Managers/EquipmentManager.cs`
8. Adicionar `OnEquipmentUpdated(long playerId, Dictionary<EquipmentSlot, IItem?> slots)` em `IWorldEvents`
9. Adicionar e implementar em `WorldManager`:
   - `ProcessEquipItem(IPlayer player, string itemId)`
   - `ProcessUnequipItem(IPlayer player, EquipmentSlot slot)`
10. Adicionar `int AttackPower` e `int Defense` ao `PlayerStatusData` DTO (default `0` em todos os sites de construção existentes)
11. Registrar `IEquipmentManager` como singleton em `Program.cs`
12. Adicionar ao `GameHub`:
    - `RequestEquipItem(string itemId)`
    - `RequestUnequipItem(string slot)`
13. Em `OnDisconnectedAsync`, chamar `_equipmentManager.Remove(player.Id)`

**Frontend:**
1. Adicionar `attackPower: number` e `defense: number` à interface `PlayerStatusData` em `types.ts`
2. Criar `managers/EquipmentManager.ts`:
   ```typescript
   export type EquipmentSlot = 'Weapon' | 'Helmet' | 'Chest' | 'Legs' | 'Boots' | 'Shield';
   export class EquipmentManager {
       private slots: Map<EquipmentSlot, ItemData | null>;
       equipItem(slot: EquipmentSlot, item: ItemData): ItemData | null;
       unequipItem(slot: EquipmentSlot): ItemData | null;
       getEquippedItem(slot: EquipmentSlot): ItemData | null;
       getSlots(): Map<EquipmentSlot, ItemData | null>;
       getTotalAttackBonus(): number;
       getTotalDefenseBonus(): number;
       onEquipmentUpdated(slots: Record<EquipmentSlot, ItemData | null>): void;
   }
   ```
3. Instanciar em `MainScene.create()`
4. `InventoryManager.useItem`: se o item for `Weapon` ou `Armor`, invocar `RequestEquipItem` em vez de `RequestUseItem`
5. Adicionar listener `"EquipmentUpdated"` em `SignalRService`
6. Atualizar `PlayerManager.updatePlayerStatus` para propagar `attackPower` e `defense` ao `Player`

### Novos eventos SignalR

| Evento | Direção | Payload |
|--------|---------|---------|
| `EquipmentUpdated` | Servidor → cliente específico | `{ slots: { Weapon: ItemData?, ... } }` |
| `RequestEquipItem` | Cliente → Servidor | `itemId: string` |
| `RequestUnequipItem` | Cliente → Servidor | `slot: string` |

---

## Fase 4 — UI de Inventário e Equipamentos (Drag-and-Drop)

### O que construir
Painéis HTML overlay (não Phaser GameObjects) — grade 4×5 para inventário (20 slots), painel de 6 slots com silhueta para equipamentos. Drag-and-drop via HTML5 API. Teclas `I`/`E` para toggle. Display reativo de stats (Ataque/Defesa).

### Testes a escrever primeiro

**Frontend — `src/__tests__/ui/InventoryUI.test.ts`**
- `render — cria 20 divs de slot`
- `render — slots começam vazios`
- `updateSlot — preenche slot com ícone e nome do item`
- `updateSlot — slot de Weapon exibe classe de ícone de espada`
- `clearSlot — remove item do slot`
- `toggle — exibe painel quando oculto`
- `toggle — oculta painel quando visível`
- `onSlotClick — callback dispara com índice do slot`
- `onSlotRightClick — callback dispara com id do item`
- `renderItems — distribui itens nos slots corretos em ordem`

**Frontend — `src/__tests__/ui/EquipmentUI.test.ts`**
- `render — cria 6 elementos de slot nomeados (weapon, helmet, chest, legs, boots, shield)`
- `updateSlot — preenche slot com dados do item`
- `clearSlot — esvazia slot`
- `toggle — exibe/oculta painel`
- `onSlotClick — dispara callback com nome do EquipmentSlot`
- `updateStats — atualiza valores exibidos de ataque e defesa`

**Frontend — `src/__tests__/ui/DragDropHandler.test.ts`**
- `canDrop_InventoryToEquipment — retorna true para arma no slot de arma`
- `canDrop_InventoryToEquipment — retorna false para armadura no slot de arma`
- `canDrop_EquipmentToInventory — sempre retorna true`
- `canDrop_InventoryToInventory — sempre retorna true`
- `handleDrop — dispara RequestEquipItem ao soltar em zona de equipamento`
- `handleDrop — dispara RequestMoveItemInInventory ao soltar em zona de inventário`

**Frontend — `src/__tests__/managers/InputManager.test.ts`**
- `isInventoryJustPressed — retorna true quando tecla I recém pressionada`
- `isEquipmentJustPressed — retorna true quando tecla E recém pressionada`

### Implementação (após os testes falharem)

1. Criar `ui/InventoryUI.ts` — gera `<div id="inventory-panel">` com 20 `<div class="inv-slot">` no `document.body`
2. Criar `ui/EquipmentUI.ts` — 6 slots com `data-slot="Weapon"` etc. e readouts `<span id="eq-attack">` e `<span id="eq-defense">`
3. Criar `ui/DragDropHandler.ts`:
   ```typescript
   export class DragDropHandler {
       constructor(
           private invUI: InventoryUI,
           private eqUI: EquipmentUI,
           private onEquip: (itemId: string) => void,
           private onUnequip: (slot: EquipmentSlot) => void,
           private onMoveInInventory: (fromIdx: number, toIdx: number) => void
       ) {}
   }
   ```
4. Adicionar `isInventoryJustPressed()` e `isEquipmentJustPressed()` ao `InputManager`
5. Instanciar `InventoryUI`, `EquipmentUI`, `DragDropHandler` em `MainScene.create()`, wiring com SignalR
6. `InventoryManager.onInventoryUpdated` → chamar `inventoryUI.renderItems(items)`
7. `EquipmentManager.onEquipmentUpdated` → chamar `equipmentUI.renderSlots(slots)` e `equipmentUI.updateStats(...)`
8. Clique direito no slot: mini menu contextual (`ui/ContextMenuUI.ts`) com opções "Usar", "Dropar", "Equipar"
9. Adicionar `RequestMoveItemInInventory(string itemId, int toIndex)` ao `GameHub` e implementar reordenação em `PlayerInventory`

### Novo evento SignalR

| Evento | Direção | Payload |
|--------|---------|---------|
| `RequestMoveItemInInventory` | Cliente → Servidor | `itemId: string, toIndex: number` |

---

## Arquivos Críticos para Implementação

| Arquivo | Fase | Mudanças |
|---------|------|----------|
| `backend/GameServerApp/Contracts/World/IItem.cs` | 1 | Adicionar `Weapon`/`Armor` ao enum, `EquipmentSlot? Slot` |
| `backend/GameServerApp/Managers/WorldManager.cs` | 1–3 | Loot table, integrar `IInventoryManager`, `IEquipmentManager` |
| `backend/GameServerApp/World/Player.cs` | 3 | `TotalAttackPower`, `TotalDefense`, `TakeDamage` com redução |
| `backend/GameServerApp/Dtos/ItemData.cs` | 1 | `AttackBonus?`, `DefenseBonus?` |
| `backend/GameServerApp/Dtos/PlayerStatusData.cs` | 3 | `AttackPower`, `Defense` |
| `frontend/src/managers/ItemManager.ts` | 1 | Ampliar para `IWorldItem`, factory dispatch |
| `frontend/src/services/SignalRService.ts` | 2–4 | Listeners e invocações para todos os novos eventos |
| `frontend/src/types.ts` | 1–3 | Campos opcionais em `ItemData`, `PlayerStatusData` |
