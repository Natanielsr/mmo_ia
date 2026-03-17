using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServerApp.Services
{
    public class MonsterMovementService : IMonsterMovementService
    {
        private readonly ICollisionManager _collisionManager;
        private readonly IMovementService _movementService;
        private readonly IPlayerManager _playerManager;
        private readonly IPathfindingService _pathfindingService;
        private readonly Random _random;
        
        // Configurações de comportamento
        private const int MOVEMENT_COOLDOWN_TICKS = 8; // Reduzido para 0.8s
        private const int MAX_MOVEMENT_RANGE = 12; 
        private const int AGGRO_RANGE = 5; 
        private const int PATHFINDING_MAX_DEPTH = 150; 

        public MonsterMovementService(
            ICollisionManager collisionManager, 
            IMovementService movementService,
            IPlayerManager playerManager,
            IPathfindingService pathfindingService)
        {
            _collisionManager = collisionManager;
            _movementService = movementService;
            _playerManager = playerManager;
            _pathfindingService = pathfindingService;
            _random = new Random();
        }

        public void UpdateAllMonsters()
        {
        }

        public void UpdateMonster(IMonster monster)
        {
            if (monster.State == MonsterState.Dead) return;
            
            // Verifica se é hora de mover (cooldown)
            if ((DateTime.UtcNow - monster.LastMovementTime).TotalSeconds < MOVEMENT_COOLDOWN_TICKS / 10.0)
                return;

            // Define o tempo do último movimento ANTES de calcular/tentar, para evitar loops infinitos se falhar
            monster.LastMovementTime = DateTime.UtcNow;

            var newPosition = CalculateNewPosition(monster);
            
            if (newPosition == monster.Position) return;

            if (!_collisionManager.IsPositionBlocked(newPosition))
            {
                var oldPosition = monster.Position;
                monster.Move(newPosition);
                _collisionManager.UpdateObjectPosition(monster, oldPosition);
            }
        }

        public Position CalculateNewPosition(IMonster monster)
        {
            var currentPosition = monster.Position;
            var spawnPosition = monster.SpawnPosition;

            var target = FindClosestPlayer(monster, AGGRO_RANGE);
            
            if (target != null)
            {
                var dx = Math.Abs(target.Position.X - currentPosition.X);
                var dy = Math.Abs(target.Position.Y - currentPosition.Y);
                if (dx <= 1 && dy <= 1) return currentPosition;

                var path = _pathfindingService.FindPath(currentPosition, target.Position, PATHFINDING_MAX_DEPTH);
                if (path != null && path.Count > 0) return path[0];
                
                return CalculateDirectMove(currentPosition, target.Position);
            }
            
            switch (monster.Behavior)
            {
                case MonsterBehavior.Patrolling:
                case MonsterBehavior.Aggressive:
                    return CalculatePatrollingPosition(monster, currentPosition, spawnPosition);
                    
                case MonsterBehavior.Passive:
                default:
                    return CalculatePassivePosition(monster, currentPosition, spawnPosition);
            }
        }

        private IPlayer? FindClosestPlayer(IMonster monster, int range)
        {
            IPlayer? closest = null;
            double minDistance = double.MaxValue;

            foreach (var player in _playerManager.GetAllPlayers())
            {
                if (player.State == PlayerState.Dead) continue;

                var dx = player.Position.X - monster.Position.X;
                var dy = player.Position.Y - monster.Position.Y;
                var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));

                if (distance <= range && distance < minDistance)
                {
                    minDistance = distance;
                    closest = player;
                }
            }

            return closest;
        }

        private Position CalculateDirectMove(Position current, Position target)
        {
            var dx = target.X - current.X;
            var dy = target.Y - current.Y;

            if (dx == 0 && dy == 0) return current;

            string direction;
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                direction = dx > 0 ? "east" : "west";
            }
            else
            {
                direction = dy > 0 ? "north" : "south";
            }

            return _movementService.Move(current, direction);
        }

        private Position CalculatePatrollingPosition(IMonster monster, Position current, Position spawn)
        {
            // Se estiver fora do alcance, volta usando Pathfinding (para não ficar preso em paredes)
            if (Math.Abs(current.X - spawn.X) > MAX_MOVEMENT_RANGE || 
                Math.Abs(current.Y - spawn.Y) > MAX_MOVEMENT_RANGE)
            {
                var path = _pathfindingService.FindPath(current, spawn, PATHFINDING_MAX_DEPTH);
                if (path != null && path.Count > 0) return path[0];
                return CalculateDirectMove(current, spawn);
            }

            // Patrulha aleatória dentro de um raio
            if (_random.NextDouble() < 0.6) // Aumentado para 60%
            {
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                
                var newPos = _movementService.Move(current, direction);
                
                if (Math.Abs(newPos.X - spawn.X) <= MAX_MOVEMENT_RANGE && 
                    Math.Abs(newPos.Y - spawn.Y) <= MAX_MOVEMENT_RANGE)
                {
                    return newPos;
                }
            }
            
            return current;
        }

        private Position CalculatePassivePosition(IMonster monster, Position current, Position spawn)
        {
            // Comportamento passivo - movimento menos frequente
            if (_random.NextDouble() < 0.1) // 10% chance de se mover
            {
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                return _movementService.Move(current, direction);
            }
            
            return current;
        }
    }
}