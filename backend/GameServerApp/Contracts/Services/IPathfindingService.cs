using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services;

public interface IPathfindingService
{
    /// <summary>
    /// Finds a path from start to goal using A* algorithm.
    /// Returns the list of positions from start (exclusive) to goal (inclusive),
    /// or null if no path is found within the search limit.
    /// </summary>
    List<Position>? FindPath(Position start, Position goal, int maxSearchDepth = 50);
}
