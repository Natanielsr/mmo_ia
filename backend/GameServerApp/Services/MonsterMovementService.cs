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
        private const int MOVEMENT_COOLDOWN_TICKS = 10; // A cada 10 ticks do sistema
        private const int MAX_MOVEMENT_RANGE = 7; // Máxima distância que um monstro pode se afastar da posição inicial
        private const int AGGRO_RANGE = 5; // Distância para começar a seguir um jogador
        private const int PATHFINDING_MAX_DEPTH = 30; // Limite de nós explorados pelo A*

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
            // Será chamado pelo WorldManager no Tick()
        }

        public void UpdateMonster(IMonster monster)
        {
            if (monster.State == MonsterState.Dead) return;
            
            // Verifica se é hora de mover (cooldown)
            if ((DateTime.UtcNow - monster.LastMovementTime).TotalSeconds < MOVEMENT_COOLDOWN_TICKS / 10.0)
                return;

            var newPosition = CalculateNewPosition(monster);
            
            // Se a posição for a mesma, não faz nada
            if (newPosition == monster.Position) return;

            // Verifica se a nova posição está bloqueada
            if (!_collisionManager.IsPositionBlocked(newPosition))
            {
                var oldPosition = monster.Position;
                monster.Move(newPosition);
                _collisionManager.UpdateObjectPosition(monster, oldPosition);
                monster.LastMovementTime = DateTime.UtcNow;
            }
        }

        public Position CalculateNewPosition(IMonster monster)
        {
            var currentPosition = monster.Position;
            var spawnPosition = monster.SpawnPosition;

            // Tenta encontrar um alvo (jogador próximo)
            var target = FindClosestPlayer(monster, AGGRO_RANGE);
            
            if (target != null)
            {
                // Usa A* para encontrar caminho livre até o jogador
                var path = _pathfindingService.FindPath(currentPosition, target.Position, PATHFINDING_MAX_DEPTH);
                
                if (path != null && path.Count > 0)
                {
                    // Retorna apenas o próximo passo do caminho
                    return path[0];
                }
                
                // Fallback: se A* não encontrou caminho, tenta mover diretamente
                return CalculateDirectMove(currentPosition, target.Position);
            }
            
            // Comportamento baseado no tipo de monstro caso não tenha alvo
            switch (monster.Behavior)
            {
                case MonsterBehavior.Patrolling:
                case MonsterBehavior.Aggressive: // Agressivo sem alvo se comporta como patrulha
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
                
                // Distância de Chebyshev (máximo entre X e Y) - comum em jogos de grid
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

            // Se já está na mesma posição, não move
            if (dx == 0 && dy == 0) return current;

            string direction;

            // Prioriza mover no eixo com maior distância
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
            // Patrulha aleatória dentro de um raio
            if (_random.NextDouble() < 0.4) // 40% chance de se mover em cada tentativa
            {
                // Escolhe direção aleatória
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                
                var newPos = _movementService.Move(current, direction);
                
                // Verifica se está dentro do alcance máximo do spawn
                if (Math.Abs(newPos.X - spawn.X) <= MAX_MOVEMENT_RANGE && 
                    Math.Abs(newPos.Y - spawn.Y) <= MAX_MOVEMENT_RANGE)
                {
                    return newPos;
                }
            }
            
            // Se estiver muito longe do spawn, tende a voltar
            if (Math.Abs(current.X - spawn.X) > MAX_MOVEMENT_RANGE || 
                Math.Abs(current.Y - spawn.Y) > MAX_MOVEMENT_RANGE)
            {
                return CalculateDirectMove(current, spawn);
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