using GameServerApp.Contracts;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using System;

namespace GameServerApp.Services.World.WorldFormations
{
    public class StoneCircleFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string?> spawnAction)
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

            if (rng.NextDouble() < 0.3)
            {
                spawnAction(centerX, centerY, ItemTags.SpawnCode.Potion);
            }
        }
    }
}
