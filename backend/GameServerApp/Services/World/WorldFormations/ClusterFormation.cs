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

            if (rng.NextDouble() < 0.2)
            {
                spawnAction(clX, clY, ItemTags.SpawnCode.Potion);
            }
        }
    }
}
