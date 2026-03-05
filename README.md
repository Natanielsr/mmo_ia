# Antigravity MMO

Um projeto de MMO em tempo real utilizando **C# (ASP.NET Core SignalR)** no backend e **Vanilla JS/CSS** no frontend.

## 🚀 Como Executar

### 1. Servidor Backend (.NET)

Para iniciar o servidor de jogo:

```bash
dotnet run --project backend/GameServer.Web/GameServer.Web.csproj
```

O servidor ficará disponível por padrão em `http://localhost:5258`.

### 2. Cliente Frontend (Web)

O frontend é uma aplicação estática. Para jogar:

1. Navegue até a pasta `frontend/`.
2. Abra o arquivo `index.html` em qualquer navegador moderno.
3. Insira o nome do seu personagem e clique em "Entrar no Jogo".

> [!TIP]
> Você pode abrir o `index.html` em múltiplas abas ou navegadores diferentes para testar a sincronização de jogadores em tempo real.

## 🧪 Testes

Para rodar a suíte de testes unitários:

```bash
dotnet test backend/GameServer.sln
```

## 🛠 Tecnologias

- **Backend**: .NET 8, SignalR (WebSockets), In-Memory Game State.
- **Frontend**: HTML5, CSS3 (Glassmorphism), JavaScript (SignalR Client).
- **Estilo**: Design moderno com fontes do Google Fonts (Inter) e transparências suaves.

---
*Desenvolvido com Antigravity*
