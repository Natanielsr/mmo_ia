using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class ClusterFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            int clX = startX + rng.Next(4, size - 4);
            int clY = startY + rng.Next(4, size - 4);
            int count = rng.Next(10, 20);
            for (int i = 0; i < count; i++)
            {
                int px = clX + rng.Next(-3, 4);
                int py = clY + rng.Next(-3, 4);
                spawnAction(px, py, null);
            }
        }
    }
}
