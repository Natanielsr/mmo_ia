---
name: project-mmo-ia-overview
description: "Visão geral do projeto Antigravity MMO — stack, arquitetura, estado atual e convenções"
metadata: 
  node_type: memory
  type: project
  originSessionId: 7b83a2a7-ddb8-4745-98b4-72cb7470dfe9
---

Antigravity MMO é um MMO em tempo real baseado em grid. Backend .NET 10 + SignalR; frontend Phaser 3 + TypeScript.

**Why:** Projeto pessoal de aprendizado/hobby com ambições de gameplay inspirado em Tibia.

**How to apply:** Usar este contexto para decisões de arquitetura, sugerir patterns compatíveis com Manager/Service/Entity, e entender que não há banco de dados ainda (estado em memória).

## Stack
- Backend: .NET 10, ASP.NET Core, SignalR WebSocket
- Frontend: Phaser 3.90, TypeScript 5.9, Vite 7.3
- Testes: xUnit + Moq (35 arquivos de teste)
- Sem banco de dados — estado em memória (IPlayerManager, IMonsterManager, etc.)

## Arquitetura
Padrão Manager/Service/Entity:
- Managers → estado mutável (PlayerManager, MonsterManager, GameStateManager...)
- Services → lógica stateless (CombatService, MovementService, AStarPathfindingService...)
- GameHub (SignalR) → ponto de entrada de rede, delega a WorldProcessor/Managers

## Portas
- Backend: 5258 (HTTP), 7163 (HTTPS)
- Frontend: 5173 (Vite)
- SignalR: ws://localhost:5258/gamehub

## Run
```bash
./run_dev.sh          # testes + ambos os servidores
dotnet test backend/GameServer.Tests/GameServer.Tests.csproj
```

## Estado atual (maio 2026)
Versão v0.1.x. Funcionalidades implementadas: movimentação grid, combate, monstros com IA (A*), geração procedural de mundo (5 formações), healing potions, chunks, speed limiting, UI glassmorphism.
