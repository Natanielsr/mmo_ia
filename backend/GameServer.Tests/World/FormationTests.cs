using GameServerApp.Services.WorldFormations;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Tests.World
{
    public class FormationTests
    {
        [Fact]
        public void StoneCircleFormation_ShouldSpawnPotion_WithProbability()
        {
            var formation = new StoneCircleFormation();
            var rng = new Random(42);
            int potionCount = 0;
            int totalRuns = 1000;

            for (int i = 0; i < totalRuns; i++)
            {
                bool spawnedInRun = false;
                formation.Generate(0, 0, 16, new Random(i), (x, y, type) => 
                {
                    if (type == "item:healing_potion") spawnedInRun = true;
                });
                if (spawnedInRun) potionCount++;
            }

            // Expect ~30% (300 in 1000). Allow some margin.
            Assert.True(potionCount > 250 && potionCount < 350, $"Potion count was {potionCount}, expected around 300");
        }

        [Fact]
        public void ClusterFormation_ShouldSpawnPotion_WithProbability()
        {
            var formation = new ClusterFormation();
            int potionCount = 0;
            int totalRuns = 1000;

            for (int i = 0; i < totalRuns; i++)
            {
                bool spawnedInRun = false;
                formation.Generate(0, 0, 16, new Random(i), (x, y, type) => 
                {
                    if (type == "item:healing_potion") spawnedInRun = true;
                });
                if (spawnedInRun) potionCount++;
            }

            // Expect ~20% (200 in 1000).
            Assert.True(potionCount > 150 && potionCount < 250, $"Potion count was {potionCount}, expected around 200");
        }

        [Fact]
        public void MazeFormation_ShouldAlwaysSpawnPotionAtEnd()
        {
            var formation = new MazeFormation();
            int potionCount = 0;
            int totalRuns = 100;

            for (int i = 0; i < totalRuns; i++)
            {
                bool spawnedAtEnd = false;
                int size = 16;
                formation.Generate(0, 0, size, new Random(i), (x, y, type) => 
                {
                    // Verifica se a poção está no final (1, 1) - Canto oposto à convergência
                    if (type == "item:healing_potion" && x == 1 && y == 1) 
                        spawnedAtEnd = true;
                });
                if (spawnedAtEnd) potionCount++;
            }

            // Deve ser 100% agora
            Assert.Equal(totalRuns, potionCount);
        }

        [Fact]
        public void RowFormation_ShouldSpawnPotion_WithProbability_AtOffset()
        {
            var formation = new RowFormation();
            int potionCount = 0;
            int totalRuns = 1000;

            for (int i = 0; i < totalRuns; i++)
            {
                bool spawnedInOffset = false;
                int size = 16;
                formation.Generate(0, 0, size, new Random(i), (x, y, type) => 
                {
                    // Verifica se a poção está no centro deslocado (size/2 + 1)
                    if (type == "item:healing_potion" && x == size/2 + 1 && y == size/2 + 1) 
                        spawnedInOffset = true;
                });
                if (spawnedInOffset) potionCount++;
            }

            // Expect ~20%
            Assert.True(potionCount > 150 && potionCount < 250);
        }

        [Fact]
        public void MazeFormation_ShouldEnsureAllPathsAreConnected()
        {
            var formation = new MazeFormation();
            int size = 16;
            var paths = new HashSet<(int, int)>();
            
            // Generate a maze chunk
            formation.Generate(0, 0, size, new Random(123), (x, y, type) => 
            {
                // We don't need to do anything here, we just want to track paths.
            });

            // Re-run to track paths (Generate doesn't return anything, so we use the same seed)
            // Actually, I'll modify the test to capture what IS NOT a wall.
            var walls = new HashSet<(int, int)>();
            formation.Generate(0, 0, size, new Random(123), (x, y, type) => 
            {
                if (type != "item:healing_potion") walls.Add((x, y));
            });

            // Find all path cells (odd, odd)
            var startCell = (1, 1);
            var queue = new Queue<(int, int)>();
            queue.Enqueue(startCell);
            var visited = new HashSet<(int, int)>();
            visited.Add(startCell);

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                
                // Check neighbors (up, down, left, right)
                int[] dx = { 0, 0, 1, -1 };
                int[] dy = { 1, -1, 0, 0 };

                for (int i = 0; i < 4; i++)
                {
                    var nx = cx + dx[i];
                    var ny = cy + dy[i];

                    if (nx >= 0 && nx < size && ny >= 0 && ny < size && !walls.Contains((nx, ny)) && !visited.Contains((nx, ny)))
                    {
                        visited.Add((nx, ny));
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            // In a grid maze with paths at odd coords, we expect all (odd, odd) cells to be reachable
            for (int x = 1; x < size - 1; x += 2)
            {
                for (int y = 1; y < size - 1; y += 2)
                {
                    Assert.True(visited.Contains((x, y)), $"Cell ({x}, {y}) should be reachable in the maze");
                }
            }
        }
    }
}
