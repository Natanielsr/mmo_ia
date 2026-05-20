---
name: feedback-tdd
description: Este projeto usa TDD — testes devem ser escritos ANTES da implementação, tanto no backend quanto no frontend
metadata:
  type: feedback
---

Este projeto adota TDD (Test-Driven Development) como abordagem obrigatória.

**Why:** O usuário quer garantir que cada funcionalidade seja pensada e especificada via teste antes de qualquer código de produção, em ambas as camadas.

**How to apply:**
- Antes de criar qualquer funcionalidade nova (backend C# ou frontend TypeScript), escrever primeiro o teste correspondente.
- Backend: xUnit em `backend/GameServer.Tests/`
- Frontend: Vitest em `frontend/mmo-frontend/src/__tests__/`
- Fluxo obrigatório: Red (teste falha) → Green (implementação mínima) → Refactor
- Nunca criar código de produção sem um teste que o cubra existir primeiro.
- Isso se aplica a: novos endpoints, novos métodos de manager/service, novas entidades, novas funções utilitárias, novos componentes de UI.
