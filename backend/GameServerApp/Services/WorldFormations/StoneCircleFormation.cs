using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class StoneCircleFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            int centerX = startX + size / 2;
            int centerY = startY + size / 2;
            int radius = rng.Next(3, 6);
            
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4;
                int px = centerX + (int)(Math.Cos(angle) * radius);
                int py = centerY + (int)(Math.Sin(angle) * radius);
                spawnAction(px, py, "pillar");
            }
        }
    }
}
