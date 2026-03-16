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
        private readonly Random _random;
        
        // Configurações de comportamento
        private const int MOVEMENT_COOLDOWN_TICKS = 10; // A cada 10 ticks do sistema
        private const int MAX_MOVEMENT_RANGE = 5; // Máxima distância que um monstro pode se afastar da posição inicial
        private const double AGGRESSIVE_CHANCE = 0.3; // 30% chance de movimento agressivo

        public MonsterMovementService(ICollisionManager collisionManager, IMovementService movementService)
        {
            _collisionManager = collisionManager;
            _movementService = movementService;
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
            
            // Comportamento baseado no tipo de monstro
            switch (monster.Behavior)
            {
                case MonsterBehavior.Patrolling:
                    return CalculatePatrollingPosition(monster, currentPosition, spawnPosition);
                    
                case MonsterBehavior.Aggressive:
                    return CalculateAggressivePosition(monster, currentPosition, spawnPosition);
                    
                case MonsterBehavior.Passive:
                default:
                    return CalculatePassivePosition(monster, currentPosition, spawnPosition);
            }
        }

        private Position CalculatePatrollingPosition(IMonster monster, Position current, Position spawn)
        {
            // Patrulha aleatória dentro de um raio
            if (_random.NextDouble() < 0.7) // 70% chance de se mover
            {
                // Escolhe direção aleatória
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                
                var newPos = _movementService.Move(current, direction);
                
                // Verifica se está dentro do alcance máximo
                if (Math.Abs(newPos.X - spawn.X) <= MAX_MOVEMENT_RANGE && 
                    Math.Abs(newPos.Y - spawn.Y) <= MAX_MOVEMENT_RANGE)
                {
                    return newPos;
                }
            }
            
            // Retorna para a posição inicial se estiver muito longe
            if (Math.Abs(current.X - spawn.X) > MAX_MOVEMENT_RANGE || 
                Math.Abs(current.Y - spawn.Y) > MAX_MOVEMENT_RANGE)
            {
                // Move em direção à posição de spawn
                var directionX = spawn.X > current.X ? "east" : "west";
                var directionY = spawn.Y > current.Y ? "south" : "north";
                
                // Prioriza mover na direção com maior diferença
                if (Math.Abs(spawn.X - current.X) > Math.Abs(spawn.Y - current.Y))
                {
                    return _movementService.Move(current, directionX);
                }
                else
                {
                    return _movementService.Move(current, directionY);
                }
            }
            
            return current; // Fica parado
        }

        private Position CalculateAggressivePosition(IMonster monster, Position current, Position spawn)
        {
            // Comportamento agressivo - tenta encontrar jogadores
            // Por enquanto, movimento aleatório com tendência agressiva
            if (_random.NextDouble() < AGGRESSIVE_CHANCE)
            {
                // Em uma implementação futura, podemos buscar jogadores próximos
                // e mover em direção a eles
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                return _movementService.Move(current, direction);
            }
            
            return CalculatePatrollingPosition(monster, current, spawn);
        }

        private Position CalculatePassivePosition(IMonster monster, Position current, Position spawn)
        {
            // Comportamento passivo - movimento menos frequente
            if (_random.NextDouble() < 0.3) // 30% chance de se mover
            {
                var directions = new[] { "north", "south", "east", "west" };
                var direction = directions[_random.Next(directions.Length)];
                return _movementService.Move(current, direction);
            }
            
            return current;
        }
    }
}