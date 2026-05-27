using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Processors
{
    public class PlayerRegenerationProcessor : IPlayerRegenerationProcessor
    {
        private readonly IPlayerManager _playerManager;
        private readonly IWorldEvents _worldEvents;
        private DateTime _lastRegenTime = DateTime.UtcNow;

        public PlayerRegenerationProcessor(IPlayerManager playerManager, IWorldEvents worldEvents)
        {
            _playerManager = playerManager;
            _worldEvents = worldEvents;
        }

        public void ProcessPlayerRegeneration()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastRegenTime).TotalSeconds < 2.0) return;

            _lastRegenTime = now;

            foreach (var player in _playerManager.GetAllPlayers())
            {
                if (player.State != PlayerState.Dead && player.Hp < player.MaxHp)
                {
                    player.Heal(0);
                    _worldEvents.OnPlayerHpChanged(new PlayerHpData
                    {
                        Id     = player.Id.ToString(),
                        Hp     = player.Hp,
                        MaxHp  = player.MaxHp,
                        IsDead = false
                    });
                }
            }
        }
    }
}
