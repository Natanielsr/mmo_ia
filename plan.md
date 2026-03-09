# Planejamento do Projeto MMO

Este arquivo serve como memória de planejamento para o projeto situado em `/home/niel/projects/mmo`.

## Resumo executivo

- Propósito: planejar e documentar decisões, arquitetura e próximos passos do projeto MMO.
- Estado atual: esboço de arquitetura e scaffolding do repositório.
- Objetivo de curto prazo: iniciar scaffold do repositório, definir stack e preparar PoC (WebSocket + movimentação básica).

## Sumário

- Objetivos Iniciais
- Etapas Propostas
- Arquitetura Sugerida para MMORPG Web
- Estrutura de Pastas Atual (Repositório Atualizado)
- Próximos Passos
- Qualidade e Testes
- Metodologia XP
- Arquitetura do Projeto (Manager/Service/Entity)
- Regras e Gameplay Inspiradas em Tibia
- Como usar este arquivo
- Histórico de Git
- Branches e Pull Requests
- Observação final

## Objetivos Iniciais

- Entender os requisitos do usuário.
- Estruturar o repositório com diretórios principais.
- Definir stack tecnológica (linguagens, frameworks, etc.).

## Etapas Propostas

1. **Análise e levantamento de requisitos**
   - Conversar com o usuário sobre funcionalidades desejadas.
   - Documentar escopo.

2. **Configuração do ambiente**
   - Inicializar repositório (Git, README, .gitignore).
   - Configurar ambiente de desenvolvimento (Python, Node, etc., conforme necessário).

3. **Estrutura do projeto**
   - Criar diretórios iniciais (src/, tests/, backend/, frontend/, docs/, etc.).
   - Esboçar módulos/core.


### Tecnologia do Backend

O backend será desenvolvido em **C# / .NET**, usando ASP.NET Core para o servidor de jogo, com suporte a WebSockets (SignalR). Um projeto xUnit ficará responsável pelos testes.

4. **Desenvolvimento iterativo**
   - Implementar funcionalidades básicas uma a uma.
   - Escrever testes.
   - Revisões e refatorações.

5. **Documentação e entrega**
   - Atualizar documentação, README, e o próprio `plan.md`.
   - Preparar deploy ou entrega conforme necessidade.

## Arquitetura Sugerida para MMORPG Web

### Componentes Principais

1. **Frontend (Cliente) – Browser**
   - **Tecnologias**: HTML5 Canvas/WebGL ou motores como Phaser/Three.js.
   - **Responsabilidades**:
     - Renderizar mapa, sprites e interface do usuário.
     - Capturar entrada do jogador (teclas, cliques).
     - Conectar-se ao backend via WebSocket para receber atualizações de estado em tempo real.
     - Gerenciar autenticação, inventário local, configurações.

2. **Servidor de Jogo (Backend)**
   - **Linguagem/Stack**: Node.js, Python (asyncio), ou outra plataforma orientada a eventos.
   - **Funções**:
     - **Servidor principal (lógica de mundo)**: processa movimentos de jogadores, combate, itens, NPCs, colisões.
     - **Gerenciamento de entidades**: mantém estados de monstros, players, objetos.
     - **WebSocket**: mantém conexões em tempo real, empurra eventos para os clientes.
     - **Sistema de zonas/instâncias**: dividir o mundo em regiões para distribuir carga.

3. **Persistência de Dados**
   - **Banco de Dados**: Relacional (PostgreSQL/MySQL) ou NoSQL (MongoDB) para armazenar contas, personagens, inventário, histórico de eventos.
   - **Cache**: Redis ou similar para estados temporários, matchmaking, sessões.

4. **Serviços Auxiliares**
   - **Autenticação/Autorização**: JWT, OAuth ou sessão via cookies.
   - **Serviço de Chat/Comunicação**: pode ser integrado ao servidor ou um microserviço separado.
   - **Logs e Monitoramento**: para performance, cheats e erros.

5. **Camada de Rede**
   - WebSockets para ações em tempo real.
   - API REST/GraphQL para operações não-urgentes (login, listagem de itens, atualizações de perfil).
   - Possível uso de UDP via WebRTC se buscarmos latência ultra-baixa.

### Fluxo de Dados Simplificado

1. **login** via REST → servidor valida → retorna token de sessão.
2. Cliente abre **WebSocket** com servidor principal.
3. Jogador envia comando (andar, atacar) → servidor processa física/estado → atualiza DB/Cache.
4. Servidor → envia atualizações de mundo para clientes na mesma zona.
5. Eventos (chat, loot, spawn) são propagados por WebSocket.

### Estrutura de Pastas Atual (Repositório Atualizado)
```text
/mmo
├── backend/
│   ├── GameServer.slnx                            # Solução .NET com todos os projetos
│   ├── GameServerApp/                            # Core do Jogo (Class Library)
│   │   ├── Auth/                                # Autenticação
│   │   ├── Contracts/                           # Interfaces e contratos
│   │   │   ├── Managers/                       # Interfaces dos Managers
│   │   │   │   ├── ICollisionManager.cs
│   │   │   │   ├── IGameStateManager.cs
│   │   │   │   ├── IWorldProcessor.cs
│   │   │   │   └── IZoneManager.cs
│   │   │   ├── Services/                       # Interfaces dos Serviços
│   │   │   │   ├── IAuthenticationService.cs
│   │   │   │   ├── ICombatService.cs
│   │   │   │   ├── IConnectionService.cs
│   │   │   │   ├── IInventoryService.cs
│   │   │   │   ├── IMovementService.cs
│   │   │   │   └── IPersistenceService.cs
│   │   │   ├── Types/                          # Tipos de domínio
│   │   │   │   ├── Position.cs
│   │   │   │   └── Size.cs
│   │   │   └── World/                          # Interfaces de entidades
│   │   │       ├── IItem.cs
│   │   │       ├── IPlayer.cs
│   │   │       ├── IWorldEvents.cs
│   │   │       └── IWorldObject.cs
│   │   ├── DB/                                 # Persistência
│   │   ├── Managers/                           # Implementações dos Managers
│   │   │   ├── CollisionManager.cs
│   │   │   ├── GameStateManager.cs
│   │   │   └── WorldProcessor.cs
│   │   ├── Network/                            # Rede
│   │   ├── World/                              # Entidades e Serviços do mundo
│   │   │   ├── CombatService.cs
│   │   │   ├── InventoryService.cs
│   │   │   ├── Item.cs
│   │   │   ├── MovementService.cs
│   │   │   ├── Player.cs
│   │   │   └── WorldObject.cs
│   │   └── GameServerApp.csproj
│   ├── GameServer.Infrastructure/              # Infraestrutura (SignalR, Database, etc.)
│   │   ├── SignalR/                            # Implementação SignalR
│   │   │   ├── GameHub.cs
│   │   │   └── SignalREventEmitter.cs
│   │   └── GameServer.Infrastructure.csproj
│   ├── GameServer.Web/                         # Host Web (ASP.NET Core)
│   │   ├── Program.cs
│   │   ├── Properties/
│   │   └── GameServer.Web.csproj
│   └── GameServer.Tests/                       # Testes xUnit
│       ├── Auth/
│       │   └── AuthenticationServiceTests.cs
│       ├── Combat/
│       │   ├── CombatImplTests.cs
│       │   └── CombatServiceTests.cs
│       ├── DB/
│       │   └── PersistenceServiceTests.cs
│       ├── Inventory/
│       │   ├── InventoryImplTests.cs
│       │   └── InventorySystemTests.cs
│       ├── Managers/
│       │   ├── CollisionImplTests.cs
│       │   ├── GameStateImplTests.cs
│       │   ├── GameStateManagerTests.cs
│       │   └── WorldProcessorImplTests.cs
│       ├── Network/
│       │   └── ConnectionServiceTests.cs
│       ├── World/
│       │   ├── MovementImplTests.cs
│       │   ├── MovementServiceTests.cs
│       │   ├── PlayerImplTests.cs
│       │   ├── PlayerTests.cs
│       │   ├── SceneObjectTests.cs
│       │   ├── WorldObjectTests.cs
│       │   └── ZoneManagerTests.cs
│       └── GameServer.Tests.csproj
├── frontend/
│   ├── mmo-frontend/                           # Frontend principal (TypeScript/Vite)
│   │   ├── src/
│   │   │   ├── game/
│   │   │   │   └── MainScene.ts               # Cena principal do jogo
│   │   │   ├── css/
│   │   │   │   └── style.css
│   │   │   ├── counter.ts
│   │   │   ├── main.ts
│   │   │   └── types.ts
│   │   ├── public/
│   │   │   ├── assets/
│   │   │   │   └── hero.png
│   │   │   └── vite.svg
│   │   ├── index.html
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   └── .gitignore
│   └── position_tester/                        # Protótipo de teste de posicionamento
│       ├── css/
│       │   └── style.css
│       ├── js/
│       │   └── game.js
│       └── index.html
├── docs/                                       # Documentação
├── MemoryContext/                              # Contexto de memória para IA
└── plan.md                                    # Este arquivo de planejamento
```

#### Organização de Projetos .NET

- **GameServerApp** (`GameServerApp.csproj`): Class Library contendo o core do jogo (entidades, serviços, managers)
- **GameServer.Infrastructure** (`GameServer.Infrastructure.csproj`): Implementações de infraestrutura (SignalR, Database)
- **GameServer.Web** (`GameServer.Web.csproj`): Host Web ASP.NET Core
- **GameServer.Tests** (`GameServer.Tests.csproj`): Testes unitários xUnit

#### Frontend Stack

- **mmo-frontend**: Frontend principal desenvolvido com:
  - TypeScript
  - Vite como bundler
  - Phaser 3 (planos futuros)
  - Interface WebSocket para conectar com o backend
- **position_tester**: Protótipo simples para testes de posicionamento e movimentação

### Próximos Passos

1. Anotar essa arquitetura no `plan.md`.
2. Definir tecnologia específica para backend e frontend.
3. Iniciar scaffold do repositório com as pastas acima.
4. Começar com um “proof of concept”: conexão WebSocket + movimentação básica.

### Qualidade e Testes

- Adotar **TDD** sempre que possível: criar testes (usando mocks) para cada função/core do jogo antes de implementar.
- Os testes devem residir em `tests/` e cobrir movimentação, combate, inventário, autenticação, etc.
- A ideia é ter a base de comportamento definida e só depois escrever a lógica real.

- **Regra de organização de código**: não definir interfaces, contratos ou tipos de domínio dentro dos arquivos de teste. Interfaces e contratos devem ser colocados em `backend/GameServerApp/Contracts/` (ou outra pasta `src/.../Contracts`) e importados pelos testes. Os testes devem apenas referenciar e mockar essas interfaces.

- **Organização de testes**: cada arquivo de teste deve testar um serviço ou responsabilidade específica. Nomes descritivos: `MovementServiceTests.cs`, `CombatServiceTests.cs`, etc. Separe em subpastas temáticas:
  - `GameServer.Tests/World/` → testes de movimentação, entidades, mapa
  - `GameServer.Tests/Combat/` → testes de combate, dano, HP
  - `GameServer.Tests/Inventory/` → testes de itens, mochila
  - etc.
- **Separação de Mocks e Implementações**: Os testes que utilizam Mocks (`*Tests.cs`) devem ser mantidos originais para validar os contratos. Testes da implementação real devem ser criados em arquivos separados com o sufixo `*ImplTests.cs`.

### Metodologia (XP - Extreme Programming)

- Adotar **Extreme Programming (XP)** como metodologia de desenvolvimento.
- Princípios-chave:
   - **Iterações curtas**: sprints muito curtos (1 semana ou menos).
   - **TDD**: testes escritos antes da implementação (já especificado acima).
   - **Pair Programming**: desenvolvimento em pares quando possível — a dupla será você e a IA: você pensa, eu executo.
   - **Continuous Integration**: builds e testes automáticos a cada commit.
   - **Refatoração contínua**: manter o código limpo e simples.
   - **Small Releases**: entregar incrementos pequenos e frequentes.
   - **On-site customer / feedback rápido**: validar requisitos com usuário frequentemente.
   - **Collective code ownership**: todos podem modificar qualquer parte do código.
   - **Sustainable pace**: evitar sobrecarga de trabalho, manter ritmo sustentável.

Essas práticas vão guiar nosso fluxo de trabalho e decisões técnicas.

### Arquitetura do Projeto (Manager/Service/Entity)

Adotamos uma abordagem que separa responsabilidades em três camadas principais:

1.  **Entidades (Domain):** Guardam dados e regras de auto-validação (ex: `Player`, `Item`).
2.  **Serviços (Stateless Services):** Especialistas em lógica pura, sem manter estado (ex: `MovementService`, `CombatService`).
3.  **Gerentes (Managers/Orchestrators):** Componentes que detêm o estado global e coordenam a interação entre Entidades e Serviços (ex: `GameStateManager`, `CollisionManager`, `WorldProcessor`).

4.  **Infraestrutura (Infrastructure):** Implementações de tecnologia específica (ex: SignalR, Database). Depende do Core.
5.  **Apresentação/Host (Web):** O ponto de entrada da aplicação que configura o pipeline do ASP.NET Core.

Essa estrutura (Clean Architecture) garante que o Core do jogo seja independente de protocolos de rede ou banco de dados.

### Regras e Gameplay Inspiradas em Tibia

- **Visão**: top-down isométrica, grid-based, com movimentação por tiles.
- **Movimentação**: o personagem se move pelo mapa usando setas/teclas WASD ou clique em tiles; arrastar e soltar não é suportado inicialmente.
- **Combate**: lutar contra monstros NPC em tempo real. Ataques baseados em distância, melee ou ranged.
- **Monstros**: criaturas com pontos de vida, dano, experiência e loot. Spawn em áreas definidas.
- **Inventário**: coleção de itens com limites de peso/espaço; usar potions para curar, equipar armas/armaduras.
- **Experiência e nível**: matar monstros dá XP; ao acumular, jogador sobe de nível e ganha mais HP/MP e atributos.
- **Mapa**: mundo dividido em regiões/zones; cada tile tem propriedades (passável, bloqueado, água, etc.).
- **Interações**: conversar com NPCs, coletar itens do chão, abrir baús.
- **Multiplayer**: sobreposição de jogadores em uma mesma instância/zone; movimentação e combate sincronizados via servidor.

*Essas regras servirão de base para os testes e o desenvolvimento inicial; poderão ser refinadas conforme avançamos.*


---

## Como usar este arquivo

- Sempre que tivermos novas decisões ou passos, adicionar registros aqui.
- Atualizar progresso das etapas e adicionar novas subseções.

---

*Criação inicial em 2 de março de 2026.*

---

### Histórico de Git

O repositório foi inicializado e commits foram feitos em etapas para refletir o desenvolvimento incremental:

1. **Inicialização do repositório e .gitignore** – adicionou `plan.md`, estrutura básica e exclusões de bin/obj.
2. **Scaffold do backend e solução .NET** – inclusão de `GameServer.slnx` e projetos.
3. **Definição de contratos** – criação de interfaces em `Contracts/` (IPlayer, serviços, objetos, tipos).
4. **Testes iniciais** – adição de testes xUnit para movimentação, combate, inventário, estado de jogo, etc.
5. **Conversão para class library** – modificação do csproj para facilitar o uso em testes.
6. **Refatorações** – separação de `Position` e `Size` em novos arquivos e remoção de warnings.
7. **Testes adicionais** – inclusão de validações de grade e restrições de inventário/obstáculos.
8. **Branch de IA** – criada `ai-branch` para trabalho assistido pela IA; futuras implementações e experimentos ocorrerão nela.

*Siga este histórico para entender a evolução do projeto.*


### Branches e Pull Requests

- `master` mantém estado de referência estável.
- `ai-branch` foi criada para desenvolvimento conduzido pela IA; após adicionar modificações é esperado abrir um pull request de volta para `master`.

*Ao terminar uma feature ou conjunto de testes nesta branch, faça o push e abra PR no GitHub.*

> **Nota de processo:** a partir de agora evitaremos commits agregados. cada modificação (contrato, teste, refatoração etc.) deve ser commitada separadamente com mensagem descritiva. 
> o commit anterior agrupou várias adições (novos contratos + testes) em único envio; isso é apenas um ponto histórico e não será repetido.


### Análise de Melhorias para o Projeto

Após analisar o projeto Antigravity MMO, identifiquei pontos positivos na arquitetura (separação clara de responsabilidades no backend, uso de interfaces e serviços injetados), mas também áreas críticas que podem ser melhoradas para escalabilidade, jogabilidade e segurança.

Abaixo estão as principais sugestões de melhoria divididas por categorias:

#### 1. Backend (.NET / SignalR)

**Validação de Movimento (Anti-Cheat):**  
No `WorldProcessor.cs`, o servidor já verifica o tempo entre movimentos baseado na velocidade do jogador. No entanto, o `SignalREventEmitter` envia a posição para todos os outros clientes assim que o movimento é processado.  
**Melhoria:** Implementar uma fila de comandos ou um buffer. Atualmente, se um jogador tiver latência alta, seus movimentos parecerão "teleportes" para os outros.

**Identificação de Jogadores:**  
O `GameHub.cs` usa `Context.ConnectionId` para o dicionário de sessões, mas `player.Name` como ID para eventos de rede. Se dois jogadores escolherem o mesmo nome, haverá conflitos visuais no frontend.  
**Melhoria já implementada:** Alterado para usar `long` (tipo `Int64`) como ID único numérico sequencial, em vez de `Guid`. O `Name` continua apenas como metadado visual.

---
### Decisões Tomadas

- **ID dos jogadores**: Implementado como `long` (tipo `Int64`), garantindo unicidade e melhor performance em serialização e persistência. IDs sequenciais numéricos são suficientes para um sistema fechado e facilitam operações com banco de dados.
- **Geração de ID**: O `Player.Id` do tipo `long` é gerado automaticamente pelo repositório/banco de dados, garantindo unicidade independente do nome escolhido pelo jogador.

**Loop de Jogo (Game Loop):**  
O `WorldProcessor.Tick()` está vazio. Em um MMO, NPCs e estados de mundo (como regeneração de HP) devem rodar em um loop fixo no servidor (ex: 20Hz ou 60Hz), independente das requisições dos clientes.  
**Melhoria:** Implementar um `BackgroundService` (.NET Worker) que chama o `Tick()` do `WorldProcessor` em intervalos fixos.

#### 2. Frontend (Phaser 3 / TypeScript)

**Interpolação e Predição (Client-Side Prediction):**  
Atualmente, o cliente envia o comando de movimento e espera a resposta do servidor para mover o sprite (`updatePlayerPosition`). Isso causa uma sensação de "input lag" (atraso entre apertar a tecla e o personagem se mover).  
**Melhoria:** Mover o sprite localmente de forma imediata (predição) e corrigir a posição caso o servidor envie um `MoveFailed` ou uma posição diferente (reconciliação).

**Gerenciamento de Assets:**  
O `MainScene.ts` carrega apenas `hero.png`. Para um MMO, será necessário um sistema de Atlas (Spritesheets) para reduzir as chamadas de rede e permitir animações de caminhada/ataque.

**Câmera e Zoom:**  
A câmera segue o jogador, mas não há limites de mundo configurados (`setBounds`), o que permite que o jogador veja o "vazio" se chegar nas bordas da grid.

#### 3. Arquitetura de Dados

**Persistência:**  
Atualmente, o estado do mundo (`_sessions`) está em um `ConcurrentDictionary` na memória do Hub. Se o servidor resetar, todo o progresso (XP, posição, inventário) é perdido.  
**Melhoria:** Integrar um banco de dados (Entity Framework com PostgreSQL ou Redis para estado em tempo real) para salvar o estado dos jogadores.

**Sincronização Inicial:**  
Ao entrar (`JoinGame`), o jogador recebe todos os outros jogadores de uma vez. Em um mapa grande, isso enviaria dados de milhares de jogadores desnecessariamente.  
**Melhoria:** Implementar AOI (Area of Interest). O servidor deve enviar apenas os dados dos jogadores que estão dentro do raio de visão do cliente.

#### 4. Experiência do Desenvolvedor (DevExp)

**Logs e Debug:**  
Faltam logs estruturados no backend (usando `ILogger`). É difícil diagnosticar problemas de colisão ou falhas de movimento sem logs no console do servidor.

**Scripts de Build:**  
O `run_dev.sh` é excelente para produtividade. Uma melhoria seria adicionar um comando de lint e type-check para o frontend antes de subir o servidor, garantindo que erros de TS não passem despercebidos.

#### Próximos Passos Sugeridos

- **Implementar Animações:** Adicionar suporte a spritesheets no Phaser para o movimento não ser apenas um deslize de imagem estática.
- **Sistema de Combate:** Expandir o `CombatService` para incluir cooldowns e diferentes tipos de ataque.
- **Persistência Básica:** Adicionar uma camada de repositório para salvar a posição do jogador ao desconectar.

#### Considerações Finais

- **MessagePack:** Usar no futuro para transporte de dados binários, melhorando performance de rede. 

