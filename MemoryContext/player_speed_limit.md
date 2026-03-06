# Lógica de Limite de Velocidade de Movimento (Speed Limit)

Este documento descreve como o limite de velocidade (Speed) de um `IPlayer` é aplicado no backend do projeto MMO. Funcionalidade adicionada para evitar que os jogadores se movam de forma irreal caso mantenham pressionadas as teclas de direção (ou enviem requisições excessivas).

## Visão Geral

O limite de velocidade é essencial num jogo de mapa baseado em _grid_ para garantir que todos fiquem restritos ao mesmo ritmo e para proteger o servidor contra eventuais _spamming_ (abuso) de requisições. O controle de velocidade foi integrado nos seguintes pontos chave do sistema:

### 1. Interface `IPlayer` e Classe Contratual `Player`
A propriedade de velocidade e o rastreamento do último movimento foram incluídos na base do personagem:

- **`double Speed`**: Define o limite de "posições virtuais" (ou casas) percorridas por segundo.
  - O valor inicial na classe `Player` foi definido como `2.0` (o que significa que o jogador pode mover-se no máximo 2 casas por segundo).
- **`DateTime LastMoveTime`**: Registra o momento exato do término do movimento mais recente.
  - Inicialmente começa como `DateTime.MinValue`.
  - É atualizado para `DateTime.UtcNow` no método `Move()` da classe `Player`.

### 2. Validação no Processador Mundial (`WorldProcessor`)
A proteção central do MMORPG está no `WorldProcessor`, responsável por processar fisicamente e emitir requisições de eventos a partir da entrada de rede (`GameHub`). 

No método `ProcessPlayerMovement`, a primeira validação lógica aplica a limitação temporal via equação:

```csharp
// Calcula o intervalo de tolerância mínimo (em segundos)
double minTimeBetweenMovesSec = 1.0 / player.Speed;

// Mede se a diferença temporal já superou ou empatou com a tolerância
if ((DateTime.UtcNow - player.LastMoveTime).TotalSeconds < minTimeBetweenMovesSec)
{
    return false; // Moving too fast (rejeita a requisição)
}
```
*   **Se for muito cedo:** O movimento não prossegue e retorna `false`, sem aplicar gasto de rede (além da recusa da ação WebSocket descrita no `GameHub`) e sem afetar a _position_ atual.
*   **Se na hora exata ou mais tarde:** O _pipeline_ é entregue ao serviço `MovementService` para conferir colisões. Em caso de aprovação, chama o `player.Move()` o qual renovará o marco em `LastMoveTime`.

### 3. Garantias por Teste Unitário
A eficácia do freio é verificada no `WorldProcessorImplTests` dentro do projeto de testes *xUnit*. O teste `Movement_Should_Fail_If_Moving_Too_Fast` assevera que movimentos em sequência sem pausa no tempo configurado devem obrigatoriamente retornar falha `false`.
