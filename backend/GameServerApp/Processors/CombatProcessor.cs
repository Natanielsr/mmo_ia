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
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Processors
{
    public class CombatProcessor : ICombatProcessor
    {
        private readonly IGameStateManager _gameStateManager;
        private readonly IWorldEvents _worldEvents;
        private readonly IMonsterManager _monsterManager;
        private readonly IPlayerManager _playerManager;
        private readonly ILootTableService _lootTableService;
        private readonly IItemManager _itemManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly WorldConfig _config;

        public CombatProcessor(
            IGameStateManager gameStateManager,
            IWorldEvents worldEvents,
            IMonsterManager monsterManager,
            IPlayerManager playerManager,
            ILootTableService lootTableService,
            IItemManager itemManager,
            IIdGeneratorService idGeneratorService,
            IOptions<WorldConfig> config)
        {
            _gameStateManager = gameStateManager;
            _worldEvents = worldEvents;
            _monsterManager = monsterManager;
            _playerManager = playerManager;
            _lootTableService = lootTableService;
            _itemManager = itemManager;
            _idGeneratorService = idGeneratorService;
            _config = config.Value;
        }

        public void ProcessPlayerAttack(IPlayer player, IPlayer target)
        {
            if (player.State == PlayerState.Dead || target.State == PlayerState.Dead) return;

            if ((DateTime.UtcNow - player.LastAttackTime).TotalSeconds < _config.Combat.AttackSpeedSec)
                return;

            player.Attack(target);
            int damage = 10;
            target.TakeDamage(damage);

            if (target.State == PlayerState.Dead)
            {
                _gameStateManager.PlayerDied(target);
                _gameStateManager.AddPlayerExperience(player, 100);
                _gameStateManager.CheckForLevelUp(player);

                _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
                {
                    Id          = player.Id.ToString(),
                    Hp          = player.Hp,
                    MaxHp       = player.MaxHp,
                    Level       = player.Level,
                    Experience  = player.Experience,
                    AttackPower = player.TotalAttackPower,
                    Defense     = player.TotalDefense
                });

                _worldEvents.OnPlayerDied(target.Id);
                _worldEvents.OnPlayerExperienceGained(player.Id, 100, player.Experience);
            }

            _worldEvents.OnPlayerAttacked(new PlayerAttackData
            {
                AttackerId   = player.Id.ToString(),
                AttackerName = player.Name,
                TargetId     = target.Id.ToString(),
                TargetName   = target.Name,
                Damage       = damage
            });
        }

        public void ProcessPlayerAttackMonster(IPlayer player, string monsterId)
        {
            if (player.State == PlayerState.Dead) return;

            if ((DateTime.UtcNow - player.LastAttackTime).TotalSeconds < _config.Combat.AttackSpeedSec)
                return;

            const int movementCooldownMs = 100;
            if ((DateTime.UtcNow - player.LastMoveTime).TotalMilliseconds < movementCooldownMs)
                return;

            if (!long.TryParse(monsterId, out long id)) return;

            var monster = _monsterManager.GetMonsterById(id);
            if (monster == null || monster.State == MonsterState.Dead) return;

            var dx = Math.Abs(player.Position.X - monster.Position.X);
            var dy = Math.Abs(player.Position.Y - monster.Position.Y);
            if (dx > 1 || dy > 1) return;

            player.Attack(monster);

            int damage = player.TotalAttackPower;
            monster.TakeDamage(damage);
            _worldEvents.OnMonsterDamaged(monster.Id.ToString(), damage, monster.Hp);

            _worldEvents.OnPlayerAttacked(new PlayerAttackData
            {
                AttackerId   = player.Id.ToString(),
                AttackerName = player.Name,
                TargetId     = monster.Id.ToString(),
                TargetName   = monster.Name,
                Damage       = damage
            });

            if (monster.IsDead)
            {
                _gameStateManager.AddPlayerExperience(player, monster.ExperienceReward);
                _gameStateManager.CheckForLevelUp(player);

                _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
                {
                    Id          = player.Id.ToString(),
                    Hp          = player.Hp,
                    MaxHp       = player.MaxHp,
                    Level       = player.Level,
                    Experience  = player.Experience,
                    AttackPower = player.TotalAttackPower,
                    Defense     = player.TotalDefense
                });

                _worldEvents.OnMonsterDied(monster.Id.ToString());

                var dropped = _lootTableService.RollLoot(
                    monster.Position,
                    _idGeneratorService.GenerateId().ToString(),
                    monster.ObjectCode);
                if (dropped != null)
                {
                    _itemManager.DropItem(dropped);
                    _worldEvents.OnItemDropped(dropped);
                }

                _monsterManager.RemoveMonster(monster.Id);
            }
        }

        public void ProcessMonsterCombat()
        {
            var monsters = _monsterManager.GetAllMonsters();
            var players  = _playerManager.GetAllPlayers();

            foreach (var monster in monsters)
            {
                if (monster.State == MonsterState.Dead || !monster.CanAttack())
                    continue;

                foreach (var player in players)
                {
                    if (player.State == PlayerState.Dead)
                        continue;

                    var dx = Math.Abs(monster.Position.X - player.Position.X);
                    var dy = Math.Abs(monster.Position.Y - player.Position.Y);

                    if (dx <= 1 && dy <= 1)
                    {
                        int effectiveDamage = Math.Max(0, monster.AttackPower - player.TotalDefense);
                        monster.Attack(player);

                        _worldEvents.OnPlayerAttacked(new PlayerAttackData
                        {
                            AttackerId   = monster.Id.ToString(),
                            AttackerName = monster.Name,
                            TargetId     = player.Id.ToString(),
                            TargetName   = player.Name,
                            Damage       = effectiveDamage
                        });

                        _worldEvents.OnPlayerHpChanged(new PlayerHpData
                        {
                            Id     = player.Id.ToString(),
                            Hp     = player.Hp,
                            MaxHp  = player.MaxHp,
                            IsDead = player.State == PlayerState.Dead
                        });

                        if (player.State == PlayerState.Dead)
                        {
                            _gameStateManager.PlayerDied(player);
                            _worldEvents.OnPlayerDied(player.Id);
                        }

                        break;
                    }
                }
            }
        }
    }
}
