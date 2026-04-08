using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class MazeFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            // Labirinto simples baseado em grid para o chunk
            // Usamos uma técnica de 'binary tree' ou 'sidewinder' simples
            // que cabe bem em um chunk isolado.

            string wallType = rng.NextDouble() > 0.5 ? "pillar" : "rock";

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Bordas do labirinto (opcional para deixar aberto entre chunks)
                    // Mas aqui vamos fazer um labirinto interno ao chunk.
                    
                    bool isWall = false;

                    // Grid básico de paredes (todos os pares são paredes em potencial)
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        isWall = true;
                    }
                    else if (x % 2 == 0) // Paredes verticais aleatórias
                    {
                        if (rng.NextDouble() < 0.7) isWall = true;
                    }
                    else if (y % 2 == 0) // Paredes horizontais aleatórias
                    {
                        if (rng.NextDouble() < 0.7) isWall = true;
                    }

                    if (isWall)
                    {
                        int wx = startX + x;
                        int wy = startY + y;
                        if (wx == 0 && wy == 0) continue;
                        spawnAction(wx, wy, wallType);
                    }
                }
            }
        }
    }
}
