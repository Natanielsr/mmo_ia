using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class MazeFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            // Binary Tree Maze Algorithm
            // Garante que todos os caminhos estejam conectados (spanning tree).
            // Usamos coordenadas ímpares como caminhos e pares como paredes potenciais.

            string wallType = rng.NextDouble() > 0.5 ? "pillar" : "rock";

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Lógica para labirinto de grid (Binary Tree)
                    // Células em (ímpar, ímpar) são espaços vazios.
                    // Outras posições são paredes que podem ser removidas.

                    bool isWall = true;

                    if (x % 2 != 0 && y % 2 != 0)
                    {
                        // É uma célula de caminho
                        isWall = false;
                        
                        // Decide remover a parede ao Norte ou ao Leste
                        bool removeNorth = rng.NextDouble() > 0.5;
                        
                        // Ajuste para as bordas do chunk
                        if (x == size - 1 || x == size - 2) removeNorth = true;
                        if (y == size - 1 || y == size - 2) removeNorth = false;
                        
                        if (removeNorth && y + 1 < size)
                        {
                            // Remove parede ao norte (spawnAction não será chamada para parede aqui)
                            // Na verdade, as paredes são o padrão, e nós "abrimos" o caminho.
                        }
                    }

                    // Nova abordagem: Preencher tudo com paredes e depois "cavar"
                    // Mas como o WorldGenerator já recebe spawnAction, vamos decidir o que colocar.
                }
            }

            // Refatorando para uma abordagem mais limpa:
            // 1. Define onde serão as paredes
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    bool isPath = false;

                    if (x % 2 != 0 && y % 2 != 0)
                    {
                        isPath = true; // Célula principal
                    }
                    else if (x % 2 != 0 && y % 2 == 0 && y > 0 && y < size - 1)
                    {
                        // Parede horizontal - pode ser aberta se a célula abaixo escolheu "Norte"
                        var cellRng = new Random(HashCode.Combine(startX + x, startY + y - 1));
                        double val = cellRng.NextDouble();
                        bool canGoEast = x < size - 2; // Célula à direita
                        
                        // Se não puder ir para o leste, FORÇA norte. Caso contrário, usa o random.
                        if (!canGoEast || val > 0.5) isPath = true;
                    }
                    else if (x % 2 == 0 && y % 2 != 0 && x > 0 && x < size - 1)
                    {
                        // Parede vertical - pode ser aberta se a célula à esquerda escolheu "Leste"
                        var cellRng = new Random(HashCode.Combine(startX + x - 1, startY + y));
                        double val = cellRng.NextDouble();
                        bool canGoNorth = y < size - 2; // Célula acima
                        
                        // Se não puder ir para o norte, FORÇA leste. Caso contrário, usa o random.
                        if (!canGoNorth || val <= 0.5) isPath = true;
                    }

                    if (!isPath)
                    {
                        int wx = startX + x;
                        int wy = startY + y;
                        if (wx == 0 && wy == 0) continue;
                        spawnAction(wx, wy, wallType);
                    }
                }
            }

            // Sempre spawna uma Poção de Cura no final do labirinto (canto inferior esquerdo oposto à raiz)
            spawnAction(startX + 1, startY + 1, "item:healing_potion");
        }
    }
}
