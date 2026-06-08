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
    public class RowFormation : IWorldFormation
    {
        public void Generate(int startX, int startY, int size, Random rng, Action<int, int, string?> spawnAction)
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

            if (rng.NextDouble() < 0.2)
            {
                // Desloca levemente do centro para não cair em cima de uma árvore/pilar da grade
                spawnAction(startX + size / 2 + 1, startY + size / 2 + 1, ItemTags.SpawnCode.Potion);
            }
        }
    }
}
