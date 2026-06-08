using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Services.Movement
{
    public class MovementService : IMovementService
    {
        private static readonly Dictionary<string, (int dx, int dy)> Directions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["north"] = (0, 1),
            ["south"] = (0, -1),
            ["east"] = (1, 0),
            ["west"] = (-1, 0),
        };

        public Position Move(Position position, string direction)
        {
            if (!Directions.TryGetValue(direction, out var delta))
                throw new ArgumentException($"Invalid direction: {direction}", nameof(direction));

            return new Position(position.X + delta.dx, position.Y + delta.dy);
        }
    }
}
