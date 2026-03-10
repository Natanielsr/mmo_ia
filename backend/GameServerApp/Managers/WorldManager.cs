// backend/GameServerApp/Managers/WorldProcessor.cs
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Managers
{
    public class WorldManager : IWorldManager
    {
        private readonly IMovementService _movementService;
        private readonly ICollisionManager _collisionManager;
        private readonly ICombatService _combatService;
        private readonly IGameStateManager _gameStateManager;
        private readonly IWorldEvents _worldEvents;
        private readonly IStaticWorldManager _staticWorldManager;

        public WorldManager(
            IMovementService movementService,
            ICollisionManager collisionManager,
            ICombatService combatService,
            IGameStateManager gameStateManager,
            IWorldEvents worldEvents,
            IStaticWorldManager staticWorldManager)
        {
            _movementService = movementService;
            _collisionManager = collisionManager;
            _combatService = combatService;
            _gameStateManager = gameStateManager;
            _worldEvents = worldEvents;
            _staticWorldManager = staticWorldManager;
        }

        // ... resto do código permanece igual
        public bool ProcessPlayerMovement(IPlayer player, string direction)
        {
            if (player.State == PlayerState.Dead) return false;

            // Enforce speed limit:
            if (player.Speed > 0)
            {
                double minTimeBetweenMovesSec = 1.0 / player.Speed;
                if ((DateTime.UtcNow - player.LastMoveTime).TotalSeconds < minTimeBetweenMovesSec)
                {
                    return false;
                }
            }

            Position targetPos = _movementService.Move(player.Position, direction);

            if (!_collisionManager.IsPositionBlocked(targetPos))
            {
                Position oldPos = player.Position;
                player.Move(targetPos);
                _collisionManager.UpdateObjectPosition(player, oldPos);

                _worldEvents.OnPlayerMoved(new PlayerPositionData() { Id = player.Id, Name = player.Name, Position = targetPos });
                return true;
            }

            return false;
        }

        public void ProcessPlayerAttack(IPlayer player, IPlayer target)
        {
            if (player.State == PlayerState.Dead || target.State == PlayerState.Dead) return;

            player.Attack(target);
            int damage = 10;
            target.TakeDamage(damage);

            if (target.State == PlayerState.Dead)
            {
                _gameStateManager.PlayerDied(target);
                _gameStateManager.AddPlayerExperience(player, 100);
                _gameStateManager.CheckForLevelUp(player);

                _worldEvents.OnPlayerDied(target.Id);
                _worldEvents.OnPlayerExperienceGained(player.Id, 100, player.Experience);
            }

            _worldEvents.OnPlayerAttacked(player.Id, target.Id, damage);
        }

        public void Tick()
        {
            // Future logic for NPCs, respawns, etc.
        }

        public void InstantiateObject(IWorldObject worldObject)
        {
            if (worldObject is IDynamicWorldObject dynamicWorldObject)
            {
                _collisionManager.RegisterDynamicObject(dynamicWorldObject);
            }
            else if (worldObject is IStaticWorldObject staticWorldObject)
            {
                _staticWorldManager.AddStaticObject(staticWorldObject);
            }
            else
            {
                throw new Exception("Invalid object type must be Dynamic or Static world object");
            }
        }
    }
}