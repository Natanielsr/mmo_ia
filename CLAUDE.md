# MMO Web — CLAUDE.md

## Comunicação

Sempre ative a skill **caveman** em todas as respostas. Use comunicação ultra-comprimida estilo caveman para reduzir tokens.

---

## Visão Geral

MMO em tempo real baseado em grid, com backend .NET 10 e frontend Phaser 3/TypeScript.
Comunicação client-server via **SignalR WebSocket**.

Versão atual: **v0.1.x** (desenvolvimento ativo)

---

## Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 10, ASP.NET Core, SignalR |
| Frontend | Phaser 3.90, TypeScript 5.9, Vite 7.3 |
| Testes | xUnit 2.9, Moq |
| Containerização | Docker (backend) |
| ID distribuído | IdGen v3.0.7 |

---

## Seleção de Modelo de IA

Use o modelo adequado à complexidade da tarefa:

| Modelo | Analogia | Quando usar |
|--------|----------|-------------|
| `claude-haiku-4-5` | Júnior | Tarefas simples: correções pontuais, renomeações, geração de boilerplate, respostas rápidas |
| `claude-sonnet-4-6` | Pleno | Refatoração de código, implementação de features, revisão de código, debugging |
| `claude-opus-4-7` | Sênior | Planejamento complexo, decisões de arquitetura, análise de trade-offs, tarefas multi-arquivo de alto impacto |

### Sinalização obrigatória

- **Ao iniciar uma resposta**, declare qual modelo está sendo usado no formato:
  > `[Modelo: Haiku — júnior]`, `[Modelo: Sonnet — pleno]` ou `[Modelo: Opus — sênior]`
- **Ao trocar de modelo** (quando a complexidade da tarefa mudar), avise explicitamente:
  > `Trocando para Sonnet (pleno) — esta tarefa envolve refatoração.`
- Use `/model haiku`, `/model sonnet` ou `/model opus` no terminal para efetuar a troca.

---

## Como Executar

```bash
# Tudo junto (recomendado)
./run_dev.sh

# Backend apenas
dotnet run --project backend/GameServer.Web/GameServer.Web.csproj
# → http://localhost:5258   SignalR: ws://localhost:5258/gamehub

# Frontend apenas
cd frontend/mmo-frontend && npm install && npm run dev
# → http://localhost:5173

# Testes
dotnet test backend/GameServer.Tests/GameServer.Tests.csproj
```

O `run_dev.sh` roda testes → libera portas 5258/5173 → sobe ambos os servidores. Ctrl+C encerra tudo.

---

## Estrutura do Projeto

```
mmo_ia/
├── backend/
│   ├── GameServerApp/          # Lógica central (C#)
│   │   ├── Contracts/          # Interfaces e configs
│   │   ├── Dtos/               # Data Transfer Objects
│   │   ├── World/              # Entidades (Player, Monster, Item, HealingPotion, StaticObject)
│   │   ├── Managers/           # Estado (GameStateManager, WorldManager, MonsterManager, ...)
│   │   └── Services/           # Lógica (CombatService, MovementService, AStarPathfindingService, ...)
│   │       └── WorldFormations/ # Geradores de formações (Row, Maze, OrganicNoise, Cluster, StoneCircle)
│   ├── GameServer.Infrastructure/
│   │   ├── SignalR/GameHub.cs  # Hub WebSocket principal
│   │   ├── SignalR/SignalREventEmitter.cs
│   │   └── Services/GameLoopService.cs  # Game loop (IHostedService)
│   ├── GameServer.Web/
│   │   ├── Program.cs          # DI, CORS, configuração
│   │   └── appsettings.json    # WorldSettings (ver abaixo)
│   └── GameServer.Tests/       # 35 arquivos xUnit
├── frontend/mmo-frontend/src/
│   ├── scenes/                 # PreloadScene, MainScene, WorldMapScene, DebugPanel, MapLoader
│   ├── managers/               # PlayerManager, MonsterManager, ItemManager, ChunkManager, InputManager
│   ├── entities/               # Player, Monster, HealingPotion (sprites Phaser)
│   ├── services/SignalRService.ts
│   ├── systems/CombatSystem.ts
│   ├── config/constants.ts
│   └── types.ts                # Interfaces TypeScript espelhando os DTOs do backend
├── MemoryContext/              # Notas de decisões de design
└── run_dev.sh
```

---

## Arquitetura

### Padrão Manager/Service/Entity

- **Managers** — estado mutável e ciclo de vida das entidades (`IGameStateManager`, `IPlayerManager`, etc.)
- **Services** — lógica pura sem estado (`ICombatService`, `IMovementService`, `IPathfindingService`, etc.)
- **Entidades** — dados com comportamentos mínimos (`Player`, `Monster`, `Item`, etc.)
- **Hub (GameHub)** — ponto de entrada de rede; delega tudo aos Managers/Processors

### Fluxo de movimento do jogador
```
Cliente → RequestMove(direction)
  → GameHub → WorldProcessor.ProcessPlayerMovement(player, direction)
    → Verifica speed limit (1.0 / player.Speed segundos entre moves)
    → MovementService (valida colisão)
    → player.Move() (atualiza LastMoveTime)
    → WorldEvents.OnPlayerMoved → SignalREventEmitter → broadcast a todos
```

### Speed Limiting
Velocidade padrão: `2.0` unidades/segundo.
Validação: `if (elapsed < 1.0 / player.Speed) return false;`
Documentado em: `MemoryContext/player_speed_limit.md`

---

## SignalR — Eventos Hub (GameHub)

### Métodos que o **cliente invoca** (Hub methods)
| Método | Parâmetro | Descrição |
|--------|-----------|-----------|
| `JoinGame` | `playerName: string` | Conecta ao mundo |
| `RequestMove` | `direction: string` | Move o jogador (`up/down/left/right`) |
| `RequestAttack` | `targetId: string` | Ataca um monstro |

### Eventos que o **servidor envia** ao cliente
| Evento | Payload | Descrição |
|--------|---------|-----------|
| `Joined` | `PlayerPositionData` | Confirmação de entrada |
| `PlayerStatusUpdated` | `PlayerStatusData` | HP, XP, nível |
| `SyncPlayers` | `PlayerPositionData[]` | Estado inicial dos outros jogadores |
| `SyncMonsters` | `MonsterData[]` | Estado inicial dos monstros |
| `SyncItems` | `ItemData[]` | Itens dropados no mundo |
| `MoveFailed` | `string` | Movimento bloqueado/inválido |
| `ChunkData` | `ChunkData` | Chunk do mapa carregado |

---

## Configuração do Mundo (appsettings.json)

```json
"WorldSettings": {
  "Map": { "Width": 32, "Height": 32, "ChunkSize": 16, "LoadRadius": 1, "SafeSpawnRadius": 4 },
  "Monsters": { "MaxGlobal": 50, "PerPlayer": 10, "MinSpawnDistance": 12, "SpawnRadius": 18, "DespawnRadius": 30, "RespawnTimeSec": 2.0 },
  "Objects": { "MinPerChunk": 1, "MaxPerChunk": 100 },
  "Combat": { "AttackSpeedSec": 0.5 }
}
```

Tweak via `appsettings.json` ou `appsettings.Development.json`.

---

## Portas

| Serviço | Porta |
|---------|-------|
| Backend HTTP | 5258 |
| Backend HTTPS | 7163 |
| Frontend Vite | 5173 |
| SignalR Hub | ws://localhost:5258/gamehub |

---

## Testes

- Framework: xUnit + Moq
- 35 arquivos de teste cobrindo: World, Managers, Services, Combat, Inventory, Auth, Network, Integration
- Testes de velocidade: `WorldProcessorImplTests.Movement_Should_Fail_If_Moving_Too_Fast`
- Sem mocks de banco (sem banco de dados ainda)

```bash
dotnet test backend/GameServer.Tests/GameServer.Tests.csproj
```

---

## Formações do Mundo (WorldFormations)

Geração procedural de obstáculos e itens via estratégias intercambiáveis:

| Formação | Arquivo | Descrição |
|----------|---------|-----------|
| Row | `RowFormation.cs` | Fileiras de obstáculos |
| Maze | `MazeFormation.cs` | Labirintos com conectividade garantida |
| OrganicNoise | `OrganicNoiseFormation.cs` | Terreno orgânico via ruído |
| Cluster | `ClusterFormation.cs` | Agrupamentos aleatórios |
| StoneCircle | `StoneCircleFormation.cs` | Círculos de pedra |

Cada formação implementa `IWorldFormation` e pode incluir spawns de `HealingPotion`.

---

## Convenções de Código

- Interfaces prefixadas com `I` (ex: `IMonsterManager`, `ICombatService`)
- DTOs sufixados com `Data` (ex: `PlayerPositionData`, `MonsterData`)
- Managers gerenciam estado; Services são stateless
- Frontend TypeScript espelha os DTOs do backend em `types.ts`
- Sem banco de dados: estado em memória apenas (por enquanto)

---

## Referências Rápidas

- Gameplay inspirado em **Tibia** (grid-based, combat, levels)
- `plan.md` — documento de planejamento completo do projeto
- `MemoryContext/` — decisões de design documentadas
