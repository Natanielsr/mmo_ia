using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class RowFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            string type = rng.NextDouble() > 0.5 ? "tree" : "pillar";
            int spacing = rng.Next(2, 4);
            for (int x = 2; x < size - 2; x += spacing)
            {
                for (int y = 2; y < size - 2; y += spacing)
                {
                    spawnAction(startX + x, startY + y, type);
                }
            }
        }
    }
}
