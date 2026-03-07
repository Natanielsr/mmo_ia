# Antigravity MMO

Um projeto de MMO em tempo real utilizando **C# (ASP.NET Core SignalR)** no backend e **Phaser 3 / TypeScript** no frontend.

## 🚀 Como Executar

A maneira mais fácil de rodar o projeto completo (testes + servidores) é usando o script de automação:

```bash
./run_dev.sh
```

Este script irá:
1. Rodar os testes unitários do backend.
2. Liberar as portas necessárias (`5258` e `5173`).
3. Iniciar o servidor backend e o dev server do frontend simultaneamente.
4. Finalizar todos os processos automaticamente ao pressionar `Ctrl+C`.

---

### Execução Manual

Se preferir rodar os componentes separadamente:

#### 1. Servidor Backend (.NET)
```bash
dotnet run --project backend/GameServer.Web/GameServer.Web.csproj
```
O servidor ficará disponível em `http://localhost:5258`.

#### 2. Cliente Frontend (Phaser/Vite)
```bash
cd frontend/mmo-frontend
npm install
npm run dev
```
O frontend ficará disponível em `http://localhost:5173` (ou a próxima porta disponível).

## 🧪 Testes

Para rodar a suíte de testes unitários manualmente:

```bash
dotnet test backend/GameServer.Tests/GameServer.Tests.csproj
```

## 🛠 Tecnologias

- **Backend**: .NET 10, SignalR (WebSockets), xUnit para testes.
- **Frontend**: Phaser 3, TypeScript, Vite como bundler.
- **Design**: UI moderna com Inter (Google Fonts) e efeitos de transparência.

---
*Desenvolvido com Antigravity*
