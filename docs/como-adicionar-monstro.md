# Como Adicionar um Novo Monstro

Este guia descreve o processo completo para adicionar um novo monstro ao jogo, seguindo a arquitetura data-driven já existente.

---

## Visão Geral

Um monstro define:
- **Stats base** — HP, ataque, XP
- **Sprite** — imagem do monstro no mundo
- **Associação com bioma** — quais biomas podem fazer spawnar este monstro
- **Loot** — quais itens são dropados ao ser derrotado

O sistema funciona assim:

```
monsters.json → MonsterDefinitionRepository (indexa por id e tagName)
            → MonsterManager.SpawnMonstersNearPosition() filtra por bioma
            → Monster entity instancia com stats
            → Frontend carrega sprite via monsterSpriteRegistry
            → OnMonsterDefeated() puxa loot-tables para drops
```

---

## Checklist

- [ ] 1. Adicionar entrada em `monsters.json`
- [ ] 2. Adicionar tagName no(s) bioma(s) em `biomes.json`
- [ ] 3. Adicionar sprite em `public/assets/<tagName>.png`
- [ ] 4. Registrar sprite em `monsterSpriteRegistry.ts`
- [ ] 5. Adicionar tabela de loot em `loot-tables.json`
- [ ] 6. Testar no jogo

---

## Passo 1 — `monsters.json`

**Arquivo:** `backend/GameServer.Web/Data/monsters.json`

Adicione uma nova entrada no array `"monsters"`:

```json
{
  "id": 5,
  "tagName": "meu-monstro",
  "name": "Meu Monstro",
  "hp": 40,
  "attack": 6,
  "xp": 30
}
```

### Campos explicados

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | int | ID único. Nunca reutilize. |
| `tagName` | string | Identificador kebab-case. Usado para sprite, bioma e loot. |
| `name` | string | Nome legível (exibido no jogo). |
| `hp` | int | Pontos de vida iniciais (quando spawna). |
| `attack` | int | Dano causado por ataque básico. |
| `xp` | int | Experiência concedida ao jogador ao derrotar. |

> **Dica de balanceamento:**
> - `rat` (nível baixo): hp=30, attack=4, xp=20
> - `wolf` (nível médio): hp=60, attack=10, xp=60
> - `orc` (nível alto): hp=90, attack=14, xp=100
> - Escale proporcionalmente: aumentar HP ~1.5x, ataque ~1.3x, XP ~1.5x

---

## Passo 2 — Associar ao(s) bioma(s)

**Arquivo:** `backend/GameServer.Web/Data/biomes.json`

Adicione o `tagName` do monstro no array `"monsterTags"` do(s) bioma(s) desejado(s):

```json
{
  "id": 1,
  "tagName": "green_field",
  "name": "Green Field",
  ...
  "monsterTags": ["rat", "wolf", "meu-monstro"],  // ← adicionar aqui
  "isDefault": true
}
```

> **Nota:** Um monstro pode aparecer em múltiplos biomas. Basta adicionar o tagName em mais de um bioma.

---

## Passo 3 — Sprite do monstro

Crie o arquivo PNG com a imagem do monstro:

```
frontend/mmo-frontend/public/assets/<tagName>.png
```

**Especificações:**
- **Tamanho:** 64×64 px (quadrado)
- **Cor de fundo:** Transparência (PNG com canal alpha)
- **Estilo:** Isométrico ou top-down, compatível com o art style do jogo

Exemplo:
```
frontend/mmo-frontend/public/assets/meu-monstro.png
```

---

## Passo 4 — Registry de sprite (Frontend)

**Arquivo:** `frontend/mmo-frontend/src/config/monsterSpriteRegistry.ts`

Adicione uma entrada mapeando o tagName para o path da sprite:

```typescript
export const MONSTER_SPRITE_REGISTRY: Record<string, string> = {
  'rat':          'assets/rat.png',
  'wolf':         'assets/wolf.png',
  'orc':          'assets/orc.png',
  'spider':       'assets/spider.png',
  'meu-monstro':  'assets/meu-monstro.png',  // ← adicionar
}
```

> A chave **deve** ser exatamente igual ao `tagName` em `monsters.json`.

---

## Passo 5 — Loot table

**Arquivo:** `backend/GameServer.Web/Data/loot-tables.json`

Adicione uma nova chave com o tagName do monstro e liste os itens que ele dropa:

```json
{
  "lootTables": {
    ...existentes...,
    "meu-monstro": [
      { "itemTag": "potion",       "weight": 20 },
      { "itemTag": "dagger",       "weight": 10 },
      { "itemTag": "raw-meat",     "weight": 15 }
    ]
  }
}
```

### Regras de peso

- Pesos são **relativos** entre as entradas do mesmo monstro
- A soma dos pesos define a chance total de drop; o restante é "no-drop"
- Ex.: pesos `[20, 10]` → totalWeight = 30 → potion 20/30 ≈ 66%, dagger 10/30 ≈ 33%

**Balanceamento:**
- Item **comum** → peso alto (ex: `20–30`)
- Item **raro** → peso baixo (ex: `5–10`)
- **No-drop** é automático se a soma dos pesos for menor que o RNG (ver lógica no `LootProcessor`)

---

## Checklist rápido

Copie e use ao adicionar um novo monstro:

```
Backend
[ ] monsters.json — nova entrada com id único + tagName kebab-case
[ ] biomes.json — adicionar tagName ao(s) bioma(s) no array monsterTags
[ ] loot-tables.json — nova chave com tagName + pesos dos itens

Frontend
[ ] public/assets/<tagName>.png — sprite 64×64 do monstro
[ ] monsterSpriteRegistry.ts — tagName → 'assets/<tagName>.png'
```

---

## Exemplo completo — `"goblin"`

Vamos adicionar um goblin que spawna em `dark_forest`:

### 1. `backend/GameServer.Web/Data/monsters.json`

```json
{
  "id": 7,
  "tagName": "goblin",
  "name": "Goblin",
  "hp": 55,
  "attack": 9,
  "xp": 50
}
```

### 2. `backend/GameServer.Web/Data/biomes.json`

```json
{
  "id": 2,
  "tagName": "dark_forest",
  ...
  "monsterTags": ["spider", "orc", "goblin"],  // ← adicionar
  ...
}
```

### 3. Asset de sprite

```
frontend/mmo-frontend/public/assets/goblin.png  (64×64, PNG com alpha)
```

### 4. `frontend/mmo-frontend/src/config/monsterSpriteRegistry.ts`

```typescript
export const MONSTER_SPRITE_REGISTRY: Record<string, string> = {
  'rat':      'assets/rat.png',
  'wolf':     'assets/wolf.png',
  'orc':      'assets/orc.png',
  'spider':   'assets/spider.png',
  'goblin':   'assets/goblin.png',  // ← adicionar
}
```

### 5. `backend/GameServer.Web/Data/loot-tables.json`

```json
{
  "lootTables": {
    ...existentes...,
    "goblin": [
      { "itemTag": "dagger",       "weight": 20 },
      { "itemTag": "monster-meat", "weight": 25 },
      { "itemTag": "leather-vest", "weight": 10 }
    ]
  }
}
```

### Verificação

1. Rodar testes:
   ```bash
   dotnet test backend/GameServer.Tests/GameServer.Tests.csproj
   ```

2. Subir backend + frontend:
   ```bash
   ./run_dev.sh
   ```

3. No jogo:
   - Navegar para `dark_forest` (fora do raio safe de 5 chunks)
   - Confirmar que goblins spawnam
   - Derrotar um goblin e verificar que itens caem corretamente

---

## Troubleshooting

| Problema | Causa | Solução |
|---------|-------|---------|
| Monstro não spawna | `tagName` não está em `monsterTags` do bioma | Adicionar em `biomes.json` |
| Sprite carrega como branco | PNG faltando em `public/assets/` | Criar arquivo ou verificar path em `monsterSpriteRegistry.ts` |
| Erro de deserialização JSON | Sintaxe inválida em `monsters.json` | Validar JSON (vírgulas, aspas, colchetes) |
| Loot não dropa | Tabela faltando em `loot-tables.json` | Adicionar entrada com tagName do monstro |
| Monstro spawna em bioma errado | `tagName` em múltiplos `monsterTags` | Remover de biomas indesejados se necessário |
