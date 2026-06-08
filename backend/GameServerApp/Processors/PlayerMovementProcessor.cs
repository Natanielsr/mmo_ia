using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Processors
{
    public class PlayerMovementProcessor : IPlayerMovementProcessor
    {
        private readonly IMovementService _movementService;
        private readonly ICollisionManager _collisionManager;
        private readonly IWorldEvents _worldEvents;
        private readonly WorldConfig _config;

        public PlayerMovementProcessor(
            IMovementService movementService,
            ICollisionManager collisionManager,
            IWorldEvents worldEvents,
            IOptions<WorldConfig> config)
        {
            _movementService = movementService;
            _collisionManager = collisionManager;
            _worldEvents = worldEvents;
            _config = config.Value;
        }

        public bool ProcessPlayerMovement(IPlayer player, string direction)
        {
            if (player.State == PlayerState.Dead) return false;

            if ((DateTime.UtcNow - player.LastAttackTime).TotalMilliseconds < 400)
                return false;

            if (player.Speed > 0)
            {
                double minTimeBetweenMovesSec = 1.0 / player.Speed;
                if ((DateTime.UtcNow - player.LastMoveTime).TotalSeconds < minTimeBetweenMovesSec)
                    return false;
            }

            var targetPos = _movementService.Move(player.Position, direction);

            if (!player.IsNoClip && _collisionManager.IsPositionBlocked(targetPos))
                return false;

            var oldPos = player.Position;
            player.Move(targetPos);
            _collisionManager.UpdateObjectPosition(player, oldPos);

            _worldEvents.OnPlayerMoved(new PlayerData
            {
                Id = player.Id.ToString(),
                Name = player.Name,
                Position = targetPos
            });

            return true;
        }

        public bool IsPlayerMoving(IPlayer player)
        {
            const int movementCooldownMs = 100;
            return (DateTime.UtcNow - player.LastMoveTime).TotalMilliseconds < movementCooldownMs;
        }
    }
}
