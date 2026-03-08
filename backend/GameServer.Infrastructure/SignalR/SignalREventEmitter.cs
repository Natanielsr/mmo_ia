using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServer.Infrastructure.SignalR
{
    public class SignalREventEmitter : IWorldEvents
    {
        private readonly IHubContext<GameHub> _hubContext;

        public SignalREventEmitter(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void OnPlayerMoved(PlayerPositionData playerPositionData)
        {
            _hubContext.Clients.All.SendAsync("PlayerMoved", playerPositionData);
        }

        public void OnPlayerJoined(PlayerPositionData playerPositionData)
        {
            _hubContext.Clients.All.SendAsync("PlayerJoined", playerPositionData);
        }

        public void OnPlayerAttacked(long attackerId, long targetId, int damage)
        {
            _hubContext.Clients.All.SendAsync("PlayerAttacked", new
            {
                AttackerId = attackerId,
                TargetId = targetId,
                Damage = damage
            });
        }

        public void OnPlayerDied(long playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerDied", playerId);
        }

        public void OnPlayerExperienceGained(long playerId, long amount, long totalExperience)
        {
            _hubContext.Clients.User(playerId.ToString()).SendAsync("ExperienceGained", new
            {
                Amount = amount,
                Total = totalExperience
            });
        }

        public void OnPlayerLevelUp(long playerId, int newLevel)
        {
            _hubContext.Clients.All.SendAsync("PlayerLevelUp", new
            {
                PlayerId = playerId,
                Level = newLevel
            });
        }

        public void OnPlayerLeft(long playerId)
        {
            _hubContext.Clients.All.SendAsync("PlayerLeft", playerId);
        }
    }
}
