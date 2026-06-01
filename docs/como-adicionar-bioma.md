# Como Adicionar um Novo Bioma

Este guia descreve o processo completo para adicionar um novo bioma ao jogo, seguindo a arquitetura data-driven já existente.

---

## Visão Geral

Um bioma define:
- **Tiles de chão** — qual prefixo de sprite usar para o terreno
- **Objetos** — quais objectCodes (árvores, pedras etc.) aparecem naquele bioma
- **Monstros** — quais tipos de monstro podem spawnar naquele bioma

O sistema funciona assim:
```
biomes.json → BiomeSelector escolhe bioma por chunk → WorldGenerator usa SemanticObjectMap
                                                    → ChunkProcessor envia BiomeTag ao cliente
                                                    → MonsterLifecycleProcessor filtra monstros por bioma
                                                    → ChunkManager renderiza tiles do bioma correto
```

---

## Checklist

- [ ] 1. Adicionar entrada em `biomes.json`
- [ ] 2. Ajustar `BiomeSelector` se necessário
- [ ] 3. Criar sprites de assets (tiles de chão + objetos)
- [ ] 4. Adicionar assets ao `BIOME_RENDER_REGISTRY` no frontend
- [ ] 5. Carregar assets no `PreloadScene`
- [ ] 6. (Opcional) Adicionar novos monstros em `monsters.json`
- [ ] 7. Testar no jogo

---

## Passo 1 — biomes.json

Arquivo: `backend/GameServer.Web/Data/biomes.json`

Adicione uma nova entrada no array `biomes`:

```json
{
  "id": 3,
  "tagName": "desert",
  "name": "Desert",
  "groundTilePrefix": "sand",
  "groundTileCount": 12,
  "semanticObjectMap": {
    "default": "cactus",
    "tree": "cactus",
    "rock": "sandstone",
    "bush": "tumbleweed",
    "pillar": "sandstone_pillar"
  },
  "monsterTags": ["scorpion", "sand_golem"],
  "isDefault": false
}
```

### Campos explicados

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | int | ID único. Nunca reutilize um ID existente. |
| `tagName` | string | Identificador kebab-case usado internamente e no frontend. |
| `name` | string | Nome legível (UI, logs). |
| `groundTilePrefix` | string | Prefixo das sprites de chão. Ex: `"sand"` → carrega `sand1.png` a `sand12.png`. |
| `groundTileCount` | int | Quantas variantes de tile existem (1..N). |
| `semanticObjectMap` | object | Mapeia tipos semânticos de objetos para os objectCodes do bioma (ver abaixo). |
| `monsterTags` | array | Lista de `tagName` de monstros que podem spawnar neste bioma. Deve existir em `monsters.json`. |
| `isDefault` | bool | Apenas **um** bioma pode ser `true`. É usado como fallback e bioma de spawn inicial. |

### SemanticObjectMap

As formações de mundo geram objetos usando tipos semânticos genéricos (`"tree"`, `"rock"`, `"bush"`, `"pillar"`, ou `null`). O `SemanticObjectMap` traduz esses genéricos para os objectCodes reais do seu bioma:

- `"default"` → usado quando a formação passa `null` (objeto aleatório)
- `"tree"` → mapeia chamadas de formação que pedem uma "árvore"
- `"rock"` → mapeia chamadas que pedem uma "pedra"
- `"bush"` → mapeia chamadas que pedem um "arbusto"
- `"pillar"` → mapeia chamadas que pedem um "pilar"

Se um tipo semântico não tiver mapeamento no bioma, ele é usado como-está (fallback seguro).

---

## Passo 2 — BiomeSelector (quando necessário)

Arquivo: `backend/GameServerApp/Services/BiomeSelector.cs`

O `BiomeSelector` usa uma função de noise 2D para escolher o bioma de cada chunk. A lógica atual:

```csharp
// Dentro do MinBiomeDistance (raio de 5 chunks da origem) → sempre green_field
// Fora + noise >= 0.55 → dark_forest
// Caso contrário → green_field (default)
```

Para adicionar um novo bioma ao critério de seleção, modifique `GetBiomeForChunk`:

```csharp
public BiomeDefinition GetBiomeForChunk(ChunkCoord coord)
{
    var defaultBiome = _biomeRepo.GetDefault();
    var distance = Math.Sqrt((double)coord.CX * coord.CX + (double)coord.CY * coord.CY);

    if (distance < MinBiomeDistance)
        return defaultBiome;

    var noise = GetNoise(coord.CX, coord.CY);

    // Dark forest: noise médio-alto
    if (noise >= 0.55 && noise < 0.80)
        return _biomeRepo.GetByTagName("dark_forest") ?? defaultBiome;

    // Desert: noise muito alto
    if (noise >= 0.80)
        return _biomeRepo.GetByTagName("desert") ?? defaultBiome;

    return defaultBiome;
}
```

> **Dica:** A função `GetNoise(cx, cy)` retorna valores entre 0 e 1 de forma determinística. Use thresholds diferentes para controlar a frequência de cada bioma.

---

## Passo 3 — Assets de sprites

Crie os arquivos PNG na pasta `frontend/mmo-frontend/public/assets/`:

### Tiles de chão
Para um bioma com `groundTilePrefix: "sand"` e `groundTileCount: 12`:
```
public/assets/sand1.png
public/assets/sand2.png
...
public/assets/sand12.png
```
- Cada tile é **64×64 px**
- São variantes do mesmo terreno (diferenças sutis de textura/cor)

### Objetos
Para cada objectCode no `semanticObjectMap`:
```
public/assets/cactus.png
public/assets/sandstone.png
public/assets/tumbleweed.png
public/assets/sandstone_pillar.png
```
- Cada sprite é **64×64 px**

---

## Passo 4 — BIOME_RENDER_REGISTRY (Frontend)

Arquivo: `frontend/mmo-frontend/src/config/biomeRenderRegistry.ts`

Adicione uma entrada para o novo bioma:

```typescript
export const BIOME_RENDER_REGISTRY: Record<string, BiomeRenderConfig> = {
    green_field: { groundTilePrefix: 'grass',     groundTileCount: 12 },
    dark_forest: { groundTilePrefix: 'darkgrass', groundTileCount: 12 },
    desert:      { groundTilePrefix: 'sand',      groundTileCount: 12 }, // novo
};
```

O `tagName` do JSON deve ser **exatamente igual** à chave aqui.

---

## Passo 5 — PreloadScene (Frontend)

Arquivo: `frontend/mmo-frontend/src/scenes/PreloadScene.ts`

Carregue os assets do novo bioma no método `preload()`:

```typescript
// Desert biome tiles
for (let i = 1; i <= 12; i++) {
    this.load.image(`sand${i}`, `assets/sand${i}.png`);
}
// Desert biome objects
this.load.image('cactus',            'assets/cactus.png');
this.load.image('sandstone',         'assets/sandstone.png');
this.load.image('tumbleweed',        'assets/tumbleweed.png');
this.load.image('sandstone_pillar',  'assets/sandstone_pillar.png');
```

> **Atenção:** Se um asset estiver faltando, o jogo vai lançar um erro imediatamente no loading screen com a mensagem:
> ```
> [BiomeAssets] Asset ausente: "sand3" → assets/sand3.png
> ```
> Isso evita que o bioma apareça broken in-game silenciosamente.

---

## Passo 6 — Novos monstros (opcional)

Se o bioma usa novos tipos de monstro que não existem ainda, adicione em `backend/GameServer.Web/Data/monsters.json`:

```json
{ "id": 5, "tagName": "scorpion", "name": "Scorpion", "hp": 50, "attack": 8, "xp": 40 }
```

E adicione o sprite do monstro em `public/assets/scorpion.png` e registre em `config/monsterSpriteRegistry.ts`:

```typescript
export const MONSTER_SPRITE_REGISTRY: Record<string, string> = {
  // ... existentes
  'scorpion': 'assets/scorpion.png',
};
```

---

## Troubleshooting

| Problema | Causa | Solução |
|---------|-------|---------|
| Erro `[BiomeAssets] Asset ausente` | PNG faltando em `public/assets/` | Crie o arquivo de sprite |
| Bioma nunca aparece no jogo | Threshold de noise muito restritivo ou `BiomeSelector` não modificado | Ajuste os thresholds em `BiomeSelector.GetBiomeForChunk` |
| Tiles aparecem como chão do bioma errado | `BIOME_RENDER_REGISTRY` ou `biomes.json` desincronizados | Verifique que `tagName` no JSON == chave no registry |
| Monstro errado spawnando no bioma | `monsterTags` no JSON aponta para tagName que não existe | Verifique `monsters.json` e garanta que o tagName existe |
| Objetos aparecem como `?` ou sprite errado | objectCode no `semanticObjectMap` não tem PNG correspondente | Crie o asset ou corrija o objectCode no JSON |
