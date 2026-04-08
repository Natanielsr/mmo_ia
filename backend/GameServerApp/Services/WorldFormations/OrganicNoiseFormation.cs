using GameServerApp.Contracts.Services;
using System;

namespace GameServerApp.Services.WorldFormations
{
    public class OrganicNoiseFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string> spawnAction)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int worldX = startX + x;
                    int worldY = startY + y;

                    if (worldX == 0 && worldY == 0) continue;

                    double noise = GetNoise(worldX, worldY);
                    double spawnChance = noise > 0.6 ? 0.4 : 0.05;

                    if (rng.NextDouble() < spawnChance)
                    {
                        spawnAction(worldX, worldY, null);
                    }
                }
            }
        }

        private double GetNoise(int x, int y)
        {
            double val = Math.Sin(x * 0.12) * Math.Cos(y * 0.15) + 
                         Math.Sin(x * 0.05 + y * 0.11) * 0.5;
            return (val + 1.5) / 3.0;
        }
    }
}
