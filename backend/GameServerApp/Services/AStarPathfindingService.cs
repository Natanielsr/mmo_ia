using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Services;

public class AStarPathfindingService : IPathfindingService
{
    private readonly ICollisionManager _collisionManager;

    // Direções: Norte, Sul, Leste, Oeste (grid 4-direcional)
    private static readonly (int dx, int dy)[] Directions =
    [
        (0, 1),   // north
        (0, -1),  // south
        (1, 0),   // east
        (-1, 0)   // west
    ];

    public AStarPathfindingService(ICollisionManager collisionManager)
    {
        _collisionManager = collisionManager;
    }

    public List<Position>? FindPath(Position start, Position goal, int maxSearchDepth = 50)
    {
        // Se start == goal, não há caminho a percorrer
        if (start == goal) return new List<Position>();

        var openSet = new PriorityQueue<Position, int>();
        var cameFrom = new Dictionary<Position, Position>();
        var gScore = new Dictionary<Position, int> { [start] = 0 };

        openSet.Enqueue(start, ManhattanDistance(start, goal));

        int nodesExplored = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            // Chegou ao destino
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            nodesExplored++;
            if (nodesExplored > maxSearchDepth)
            {
                // Limite de busca atingido — retorna null (sem caminho viável no budget)
                return null;
            }

            var currentG = gScore[current];

            foreach (var (dx, dy) in Directions)
            {
                var neighbor = new Position(current.X + dx, current.Y + dy);

                // Permite o goal mesmo que esteja "bloqueado" (é onde o player está)
                if (neighbor != goal && _collisionManager.IsPositionBlocked(neighbor))
                    continue;

                var tentativeG = currentG + 1; // Custo uniforme de 1 por tile

                if (!gScore.TryGetValue(neighbor, out var existingG) || tentativeG < existingG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;

                    var fScore = tentativeG + ManhattanDistance(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        // Nenhum caminho encontrado
        return null;
    }

    private static int ManhattanDistance(Position a, Position b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static List<Position> ReconstructPath(Dictionary<Position, Position> cameFrom, Position current)
    {
        var path = new List<Position> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();

        // Remove o start (posição 0) — retornamos apenas os próximos passos
        if (path.Count > 0)
            path.RemoveAt(0);

        return path;
    }
}
