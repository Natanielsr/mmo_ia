using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class WorldProcessor : IWorldProcessor
    {
        private readonly IMovementService _movementService;
        private readonly ICollisionManager _collisionManager;
        private readonly ICombatService _combatService;
        private readonly IGameStateManager _gameStateManager;

        public WorldProcessor(
            IMovementService movementService,
            ICollisionManager collisionManager,
            ICombatService combatService,
            IGameStateManager gameStateManager)
        {
            _movementService = movementService;
            _collisionManager = collisionManager;
            _combatService = combatService;
            _gameStateManager = gameStateManager;
        }

        public bool ProcessPlayerMovement(IPlayer player, string direction)
        {
            if (player.State == PlayerState.Dead) return false;

            // 1. Calculate target position
            Position targetPos = _movementService.Move(player.Position, direction);

            // 2. Validate collision
            if (!_collisionManager.IsPositionBlocked(targetPos))
            {
                // 3. Update player position
                player.Move(targetPos);
                return true;
            }

            return false;
        }

        public void ProcessPlayerAttack(IPlayer player, IPlayer target)
        {
            if (player.State == PlayerState.Dead || target.State == PlayerState.Dead) return;

            // Simple combat logic integration
            // In a real scenario, this would involve more calculations
            player.Attack(target);
            
            // Apply damage using CombatService
            // Note: Our CombatService currently just returns the new HP, 
            // the Player class handles the actual TakeDamage.
            // We could refine this integration later.
            int damage = 10; // Placeholder damage
            target.TakeDamage(damage);

            if (target.State == PlayerState.Dead)
            {
                _gameStateManager.PlayerDied(target);
                _gameStateManager.AddPlayerExperience(player, 100); // 100 XP reward
                _gameStateManager.CheckForLevelUp(player);
            }
        }

        public void Tick()
        {
            // Future logic for NPCs, respawns, etc.
        }
    }
}
