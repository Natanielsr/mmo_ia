using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Processors
{
    public class PlayerHoTProcessor : IPlayerHoTProcessor
    {
        private readonly IPlayerManager _playerManager;
        private readonly IWorldEvents _worldEvents;
        private DateTime _lastTick = DateTime.UtcNow;

        public PlayerHoTProcessor(IPlayerManager playerManager, IWorldEvents worldEvents)
        {
            _playerManager = playerManager;
            _worldEvents = worldEvents;
        }

        public void ProcessHoT()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastTick).TotalSeconds < 1.0) return;
            _lastTick = now;

            foreach (var player in _playerManager.GetAllPlayers())
            {
                if (player.State == PlayerState.Dead) continue;
                bool healed = player.TickHoT();
                if (healed)
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
