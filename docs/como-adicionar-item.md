# Como Adicionar um Novo Item

## Fluxo geral

```
items.json
  → ItemDefinitionRepository (carrega/indexa por id e tagName)
  → ItemFactory.Create()
  → ItemManager.DropItem()
  → IWorldEvents.OnItemDropped()
  → SignalR "ItemDropped" → Frontend
  → ItemManager (frontend) → Sprite no mundo
  → OverlayRegistry → Animação no player quando equipado
```

---

## Tipos disponíveis

| `type` no JSON | Slot de equipamento | Bônus |
|----------------|---------------------|-------|
| `Potion`   | — (consumível)    | `healAmount` |
| `Weapon`   | Weapon            | `attackBonus` |
| `Armor`    | Chest             | `defenseBonus` |
| `Helmet`   | Helmet            | `defenseBonus` |
| `Shield`   | Shield            | `defenseBonus` |
| `Legs`     | Legs              | `defenseBonus` |
| `Boots`    | Boots             | `defenseBonus` |

---

## Passo a passo

### 1. Backend — Catálogo JSON

**Arquivo:** `backend/GameServer.Web/Data/items.json`

Adicionar entrada no array `"items"`:

```json
{
  "id": 8,
  "tagName": "meu-item",
  "name": "Nome do Item",
  "description": "Descrição curta",
  "value": 30,
  "type": "Weapon",
  "weight": 1.5,
  "attackBonus": 8
}
```

> **Regras:**
> - `id` deve ser único (inteiro, incremental)
> - `tagName` deve ser único, kebab-case
> - Inclua apenas o campo de bônus correspondente ao tipo

---

### 2. Backend — Constante de tag

**Arquivo:** `backend/GameServerApp/Contracts/ItemTags.cs`

```csharp
public const string MeuItem = "meu-item";
```

Se o item deve spawnar via `WorldFormations`, adicionar também em `SpawnCode`:

```csharp
public static class SpawnCode
{
    public const string Potion  = "item:" + ItemTags.Potion;
    public const string MeuItem = "item:" + ItemTags.MeuItem;  // ← se aplicável
}
```

---

### 3. Backend — Loot table (se o item deve dropar de monstros)

**Arquivo:** `backend/GameServer.Web/Data/loot-tables.json`

Adicionar o `tagName` do item na lista de drops do monstro desejado com um `weight` relativo:

```json
"orc": [
  { "itemTag": "meu-item", "weight": 10 }
]
```

> **Regras de peso:**
> - Pesos são **relativos** entre as entradas do mesmo monstro
> - A soma dos pesos define a chance total de drop; o restante até `totalWeight` é "no drop"
> - Ex.: pesos `[60, 10]` → totalWeight = 70 → potion 60/70 ≈ 85.7 %, dagger 10/70 ≈ 14.3 %, no-drop ≈ 30 % (se o rng escalar além de 70)
> - Para item raro, use peso baixo (ex: `5`); para item comum, use peso alto (ex: `60`)

---

### 4. Backend — Novo tipo (apenas se necessário)

> Pule esta etapa se o tipo já existe (ver tabela acima).

Se o item tem comportamento único (ex: novo consumível), criar:

- Entidade em `backend/GameServerApp/World/MeuItem.cs` herdando de `Item`
- Registrar no switch de `backend/GameServerApp/Services/ItemFactory.cs`
- Adicionar valor no enum `ItemType` em `backend/GameServerApp/Contracts/World/IItem.cs`

---

### 4. Frontend — Ícone de inventário

**Arquivo:** `frontend/mmo-frontend/src/config/itemIconRegistry.ts`

```typescript
export const ITEM_ICON_REGISTRY: Record<string, string> = {
  // ...existentes...
  'meu-item': '/assets/items_icon/meu-item.png',
}
```

---

### 5. Frontend — Asset do ícone

Adicionar o PNG do ícone (aparece no inventário/UI):

```
frontend/mmo-frontend/public/assets/items_icon/meu-item.png
```

---

### 6. Frontend — Overlay de equipamento (apenas se equipment)

> Pule esta etapa para itens consumíveis (Potion).

Adicionar `OverlayConfig` no registry do slot correspondente:

| Slot | Arquivo |
|------|---------|
| Weapon | `frontend/mmo-frontend/src/config/overlays/weaponOverlayRegistry.ts` |
| Armor (Chest) | `frontend/mmo-frontend/src/config/overlays/armorOverlayRegistry.ts` |
| Helmet | `frontend/mmo-frontend/src/config/overlays/headOverlayRegistry.ts` |
| Legs | `frontend/mmo-frontend/src/config/overlays/legsOverlayRegistry.ts` |
| Boots | `frontend/mmo-frontend/src/config/overlays/feetOverlayRegistry.ts` |
| Shield | `frontend/mmo-frontend/src/config/overlays/shieldOverlayRegistry.ts` |

Exemplo para um Weapon:

```typescript
'meu-item': {
    walkTextureKey:  'weapon_meu_item_walk',
    walkAssetPath:   'assets/walkcycle/WEAPON_meu_item.png',
    slashTextureKey: 'weapon_meu_item_slash',
    slashAssetPath:  'assets/slash/WEAPON_meu_item.png',
    frameWidth:  64,
    frameHeight: 64,
    walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
    slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
    walkFrameRate:  16,
    slashFrameRate: 24,
}
```

> Os intervalos de frames (`north: [1, 8]` etc.) devem corresponder ao layout do spritesheet. Use os valores do `dagger` como referência se o sheet seguir o mesmo padrão.

---

### 7. Frontend — Assets de animação do overlay (apenas se equipment)

Adicionar os spritesheets de animação do personagem equipando o item:

```
frontend/mmo-frontend/public/assets/walkcycle/WEAPON_meu_item.png   ← obrigatório
frontend/mmo-frontend/public/assets/slash/WEAPON_meu_item.png        ← obrigatório
frontend/mmo-frontend/public/assets/hurt/WEAPON_meu_item.png         ← opcional
```

> Nomenclatura por slot:
> - Weapon → `WEAPON_<nome>.png`
> - Armor/Chest → `TORSO_<nome>.png`
> - Helmet → `HEAD_<nome>.png`
> - Legs → `LEGS_<nome>.png`
> - Boots → `FEET_<nome>.png`
> - Shield → `SHIELD_<nome>.png`

---

## Checklist rápido

Copie e use ao adicionar qualquer item:

```
Backend
[ ] items.json — nova entrada com id único
[ ] ItemTags.cs — nova constante (+ SpawnCode se for spawnar no mundo)
[ ] loot-tables.json — adicionar entry no(s) monstro(s) que devem dropar o item
[ ] (se novo tipo) World/<Tipo>.cs + ItemFactory + enum ItemType

Frontend
[ ] itemIconRegistry.ts — tagName → path do ícone
[ ] public/assets/items_icon/<tagName>.png — ícone
[ ] (se equipment) overlay registry do slot — OverlayConfig
[ ] (se equipment) public/assets/walkcycle/<OVERLAY>.png
[ ] (se equipment) public/assets/slash/<OVERLAY>.png
[ ] (se equipment) public/assets/hurt/<OVERLAY>.png (opcional)
```

---

## Exemplo completo — `"sword"` (Weapon)

### 1. `backend/GameServer.Web/Data/items.json`

```json
{
  "id": 8,
  "tagName": "sword",
  "name": "Iron Sword",
  "description": "Uma espada de ferro resistente",
  "value": 40,
  "type": "Weapon",
  "weight": 2.0,
  "attackBonus": 10
}
```

### 2. `backend/GameServerApp/Contracts/ItemTags.cs`

```csharp
public const string Dagger = "dagger";
public const string Sword  = "sword";   // ← adicionar
```

### 3. `backend/GameServer.Web/Data/loot-tables.json`

```json
"orc": [
  { "itemTag": "potion", "weight": 20 },
  { "itemTag": "sword",  "weight": 8 }   // ← adicionar
]
```

### 5. `frontend/mmo-frontend/src/config/itemIconRegistry.ts`

```typescript
'dagger': '/assets/items_icon/dagger.png',
'sword':  '/assets/items_icon/sword.png',  // ← adicionar
```

### 6. Asset de ícone

```
frontend/mmo-frontend/public/assets/items_icon/sword.png
```

### 7. `frontend/mmo-frontend/src/config/overlays/weaponOverlayRegistry.ts`

```typescript
export const WEAPON_OVERLAY_REGISTRY: Record<string, OverlayConfig> = {
    'dagger': { /* ... existente ... */ },
    'sword': {
        walkTextureKey:  'weapon_sword_walk',
        walkAssetPath:   'assets/walkcycle/WEAPON_sword.png',
        slashTextureKey: 'weapon_sword_slash',
        slashAssetPath:  'assets/slash/WEAPON_sword.png',
        frameWidth:  64,
        frameHeight: 64,
        walkDirectionFrames:  { north: [1, 8], west: [10, 17], south: [19, 26], east: [28, 35] },
        slashDirectionFrames: { north: [0, 5], west: [6, 11], south: [12, 17], east: [18, 23] },
        walkFrameRate:  16,
        slashFrameRate: 24,
    }
}
```

### 8. Assets de animação

```
frontend/mmo-frontend/public/assets/walkcycle/WEAPON_sword.png
frontend/mmo-frontend/public/assets/slash/WEAPON_sword.png
frontend/mmo-frontend/public/assets/hurt/WEAPON_sword.png   (opcional)
```

> Frame layout igual ao `WEAPON_dagger.png`.
