using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Processors
{
    public class MonsterLifecycleProcessor : IMonsterLifecycleProcessor
    {
        private readonly IMonsterManager _monsterManager;
        private readonly IPlayerManager _playerManager;
        private readonly IMonsterMovementService _monsterMovementService;
        private readonly IWorldEvents _worldEvents;
        private readonly IBiomeSelector _biomeSelector;
        private readonly WorldConfig _config;
        private readonly List<DateTime> _pendingRespawns = new();

        public MonsterLifecycleProcessor(
            IMonsterManager monsterManager,
            IPlayerManager playerManager,
            IMonsterMovementService monsterMovementService,
            IWorldEvents worldEvents,
            IBiomeSelector biomeSelector,
            IOptions<WorldConfig> config)
        {
            _monsterManager = monsterManager;
            _playerManager = playerManager;
            _monsterMovementService = monsterMovementService;
            _worldEvents = worldEvents;
            _biomeSelector = biomeSelector;
            _config = config.Value;
        }

        public void ProcessMonsterMovement()
        {
            var monsters = _monsterManager.GetAllMonsters();
            foreach (var monster in monsters)
            {
                if (monster.State == MonsterState.Dead || !monster.CanMove())
                    continue;

                _monsterMovementService.UpdateMonster(monster);

                _worldEvents.OnMonsterMoved(new MonsterData
                {
                    Id          = monster.Id.ToString(),
                    Name        = monster.Name,
                    ObjectCode  = monster.ObjectCode,
                    Position    = monster.Position,
                    Hp          = monster.Hp,
                    MaxHp       = monster.MaxHp,
                    AttackPower = monster.AttackPower,
                    IsDead      = monster.IsDead
                });
            }
        }

        public void ProcessMonsterRespawn()
        {
            var now = DateTime.UtcNow;
            var players = _playerManager.GetAllPlayers().ToList();
            if (players.Count == 0) return;

            _pendingRespawns.RemoveAll(t => t <= now);

            var currentMonsters = _monsterManager.GetAllMonsters().ToList();
            var radiusSq  = _config.Monsters.DespawnRadius * _config.Monsters.DespawnRadius;
            var minRadius = Math.Max(_config.Map.SafeSpawnRadius + 1, _config.Monsters.MinSpawnDistance);

            const int absoluteMax = 500;

            foreach (var player in players)
            {
                if (currentMonsters.Count >= absoluteMax) break;

                var monstersNearCount = currentMonsters.Count(m =>
                {
                    var dx = m.Position.X - player.Position.X;
                    var dy = m.Position.Y - player.Position.Y;
                    return (dx * dx + dy * dy) <= radiusSq;
                });

                if (monstersNearCount < _config.Monsters.PerPlayer)
                {
                    int needed = _config.Monsters.PerPlayer - monstersNearCount;

                    var biome = _biomeSelector.GetBiomeForPosition(player.Position);
                    var spawned = _monsterManager.SpawnMonstersNearPosition(
                        needed,
                        player.Position,
                        minRadius,
                        _config.Monsters.SpawnRadius,
                        monsterTagsFilter: biome.MonsterTags,
                        positionValidator: pos => _biomeSelector.GetBiomeForTile(pos.X, pos.Y).TagName == biome.TagName);

                    if (spawned != null && spawned.Any())
                    {
                        foreach (var monster in spawned)
                        {
                            currentMonsters.Add(monster);

                            _worldEvents.OnMonsterSpawned(new MonsterData
                            {
                                Id          = monster.Id.ToString(),
                                Name        = monster.Name,
                                ObjectCode  = monster.ObjectCode,
                                Position    = monster.Position,
                                Hp          = monster.Hp,
                                MaxHp       = monster.MaxHp,
                                AttackPower = monster.AttackPower,
                                IsDead      = monster.IsDead
                            });
                        }
                    }
                }
            }
        }

        public void ProcessMonsterDespawn()
        {
            var monsters = _monsterManager.GetAllMonsters().ToList();
            var players  = _playerManager.GetAllPlayers().ToList();

            if (players.Count == 0)
            {
                foreach (var monster in monsters)
                {
                    _monsterManager.RemoveMonster(monster.Id);
                    _worldEvents.OnMonsterRemoved(monster.Id.ToString());
                    _pendingRespawns.Add(DateTime.UtcNow.AddSeconds(_config.Monsters.RespawnTimeSec));
                }
                return;
            }

            foreach (var monster in monsters)
            {
                bool nearAnyPlayer = false;
                foreach (var player in players)
                {
                    var dx   = Math.Abs(monster.Position.X - player.Position.X);
                    var dy   = Math.Abs(monster.Position.Y - player.Position.Y);
                    var dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist <= _config.Monsters.DespawnRadius)
                    {
                        nearAnyPlayer = true;
                        break;
                    }
                }

                if (!nearAnyPlayer)
                {
                    _monsterManager.RemoveMonster(monster.Id);
                    _worldEvents.OnMonsterRemoved(monster.Id.ToString());
                    _pendingRespawns.Add(DateTime.UtcNow.AddSeconds(_config.Monsters.RespawnTimeSec));
                }
            }
        }

        public void ScheduleRespawn(double delaySec)
        {
            _pendingRespawns.Add(DateTime.UtcNow.AddSeconds(delaySec));
        }
    }
}
