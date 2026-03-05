using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServer.Infrastructure.SignalR
{
    public class SignalREventEmitter : IWorldEvents
    {
        private readonly IHubContext<GameHub> _hubContext;

        public SignalREventEmitter(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void OnPlayerMoved(string playerId, Position newPosition)
        {
            _hubContext.Clients.All.SendAsync("PlayerMoved", new
            {
                PlayerId = playerId,
                X = newPosition.X,
                Y = newPosition.Y
            });
        }

        public void OnPlayerAttacked(string attackerId, string targetId, int damage)
        {
            _hubContext.Clients.All.SendAsync("PlayerAttacked", new
            {
                AttackerId = attackerId,
                TargetId = targetId,
                Damage = damage
            });
        }

        public void OnPlayerDied(string playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerDied", playerId);
        }

        public void OnPlayerExperienceGained(string playerId, long amount, long totalExperience)
        {
            _hubContext.Clients.User(playerId).SendAsync("ExperienceGained", new
            {
                Amount = amount,
                Total = totalExperience
            });
        }

        public void OnPlayerLevelUp(string playerId, int newLevel)
        {
            _hubContext.Clients.All.SendAsync("PlayerLevelUp", new
            {
                PlayerId = playerId,
                Level = newLevel
            });
        }

        public void OnPlayerLeft(string playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerLeft", playerId);
        }
    }
}
