// backend/GameServerApp/Managers/WorldProcessor.cs
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;
using Microsoft.Extensions.Options;

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
        private readonly IMonsterMovementService _monsterMovementService;
        private readonly IMonsterManager _monsterManager;
        private readonly IPlayerManager _playerManager;
        private readonly IItemManager _itemManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly IWorldGenerator _worldGenerator;
        private readonly WorldConfig _config;
        private readonly List<DateTime> _pendingRespawns = new();
        private DateTime _lastRegenTime = DateTime.UtcNow;

        public WorldManager(
            IMovementService movementService,
            ICollisionManager collisionManager,
            ICombatService combatService,
            IGameStateManager gameStateManager,
            IWorldEvents worldEvents,
            IStaticWorldManager staticWorldManager,
            IMonsterMovementService monsterMovementService,
            IMonsterManager monsterManager,
            IPlayerManager playerManager,
            IItemManager itemManager,
            IIdGeneratorService idGeneratorService,
            IWorldGenerator worldGenerator,
            IOptions<WorldConfig> config)
        {
            _movementService = movementService;
            _collisionManager = collisionManager;
            _combatService = combatService;
            _gameStateManager = gameStateManager;
            _worldEvents = worldEvents;
            _staticWorldManager = staticWorldManager;
            _monsterMovementService = monsterMovementService;
            _monsterManager = monsterManager;
            _playerManager = playerManager;
            _itemManager = itemManager;
            _idGeneratorService = idGeneratorService;
            _worldGenerator = worldGenerator;
            _config = config.Value;
        }

        // ... resto do código permanece igual
        public bool ProcessPlayerMovement(IPlayer player, string direction)
        {
            if (player.State == PlayerState.Dead) return false;

            // Block movement during attack animation (approx 400ms)
            if ((DateTime.UtcNow - player.LastAttackTime).TotalMilliseconds < 400)
            {
                return false;
            }

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

                _worldEvents.OnPlayerMoved(new PlayerPositionData() { Id = player.Id.ToString(), Name = player.Name, Position = targetPos });
                
                var connId = _playerManager.GetConnectionIdByPlayerId(player.Id);
                if (connId != null) ProcessChunkLoading(player, connId);
                ProcessItemPickup(player);
                return true;
            }

            return false;
        }

        public void ProcessPlayerAttack(IPlayer player, IPlayer target)
        {
            if (player.State == PlayerState.Dead || target.State == PlayerState.Dead) return;

            // Verifica cooldown de ataque
            if ((DateTime.UtcNow - player.LastAttackTime).TotalSeconds < _config.Combat.AttackSpeedSec)
            {
                return;
            }

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
                    Id = player.Id.ToString(),
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Level = player.Level,
                    Experience = player.Experience
                });

                _worldEvents.OnPlayerDied(target.Id);
                _worldEvents.OnPlayerExperienceGained(player.Id, 100, player.Experience);
            }

            _worldEvents.OnPlayerAttacked(new PlayerAttackData
            {
                AttackerId = player.Id.ToString(),
                AttackerName = player.Name,
                TargetId = target.Id.ToString(),
                TargetName = target.Name,
                Damage = damage
            });
        }

        public void ProcessPlayerAttackMonster(IPlayer player, string monsterId)
        {
            if (player.State == PlayerState.Dead) return;

            // Verifica cooldown de ataque
            if ((DateTime.UtcNow - player.LastAttackTime).TotalSeconds < _config.Combat.AttackSpeedSec)
            {
                return;
            }

            // Não pode atacar enquanto está se movendo
            if (IsPlayerMoving(player))
            {
                return;
            }

            if (!long.TryParse(monsterId, out long id)) return;

            var monster = _monsterManager.GetMonsterById(id);
            if (monster == null || monster.State == MonsterState.Dead) return;

            // Validate distance (adjacent)
            var dx = Math.Abs(player.Position.X - monster.Position.X);
            var dy = Math.Abs(player.Position.Y - monster.Position.Y);

            if (dx > 1 || dy > 1) return;

            player.Attack(monster); // player.Attack(IPlayer) expects player, but works for state change

            int damage = player.AttackPoints;
            monster.TakeDamage(damage);
            _worldEvents.OnMonsterDamaged(monster.Id.ToString(), damage, monster.Hp);

            _worldEvents.OnPlayerAttacked(new PlayerAttackData
            {
                AttackerId = player.Id.ToString(),
                AttackerName = player.Name,
                TargetId = monster.Id.ToString(),
                TargetName = monster.Name,
                Damage = damage
            });

            if (monster.IsDead)
            {
                _gameStateManager.AddPlayerExperience(player, monster.ExperienceReward); // XP specific for each monster type
                _gameStateManager.CheckForLevelUp(player);

                _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
                {
                    Id = player.Id.ToString(),
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Level = player.Level,
                    Experience = player.Experience
                });

                _worldEvents.OnMonsterDied(monster.Id.ToString());

                // Roll for drop (30% chance for healing potion)
                if (Random.Shared.NextDouble() < 0.3)
                {
                    var potion = new HealingPotion(
                        id: _idGeneratorService.GenerateId().ToString(),
                        position: monster.Position
                    );
                    _itemManager.DropItem(potion);
                    _worldEvents.OnItemDropped(potion);
                }

                // Remove o monstro do manager para liberar a colisão e memória
                _monsterManager.RemoveMonster(monster.Id);

                // Agenda o respawn
                _pendingRespawns.Add(DateTime.UtcNow.AddSeconds(_config.Monsters.RespawnTimeSec));
            }
        }

        public void Tick()
        {
            // Processa movimento dos monstros
            ProcessMonsterMovement();

            // Processa ataques dos monstros
            ProcessMonsterCombat();

            // Despawn de monstros longe do player
            ProcessMonsterDespawn();

            // Respawn de monstros mortos
            ProcessMonsterRespawn();
            
            // Regeneração de player
            ProcessPlayerRegeneration();
        }

        private void ProcessMonsterMovement()
        {
            var monsters = _monsterManager.GetAllMonsters();
            foreach (var monster in monsters)
            {
                if (monster.State != MonsterState.Dead && monster.CanMove())
                {
                    _monsterMovementService.UpdateMonster(monster);

                    // Notifica os clientes sobre o movimento do monstro
                    _worldEvents.OnMonsterMoved(new MonsterData
                    {
                        Id = monster.Id.ToString(),
                        Name = monster.Name,
                        ObjectCode = monster.ObjectCode,
                        Position = monster.Position,
                        Hp = monster.Hp,
                        MaxHp = monster.MaxHp,
                        AttackPower = monster.AttackPower,
                        IsDead = monster.IsDead
                    });
                }
            }
        }

        private void ProcessMonsterCombat()
        {
            var monsters = _monsterManager.GetAllMonsters();
            var players = _playerManager.GetAllPlayers();

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

                    // Adjacente (incluindo diagonais se for grid de 1 casa)
                    if (dx <= 1 && dy <= 1)
                    {
                        monster.Attack(player);
                        _worldEvents.OnPlayerAttacked(new PlayerAttackData
                        {
                            AttackerId = monster.Id.ToString(),
                            AttackerName = monster.Name,
                            TargetId = player.Id.ToString(),
                            TargetName = player.Name,
                            Damage = monster.AttackPower
                        });

                        if (player.State == PlayerState.Dead)
                        {
                            _gameStateManager.PlayerDied(player);
                            _worldEvents.OnPlayerDied(player.Id);
                        }

                        // O monstro ataca apenas um player por tick
                        break;
                    }
                }
            }
        }

        private void ProcessPlayerRegeneration()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastRegenTime).TotalSeconds < 2.0) return;

            _lastRegenTime = now;

            var players = _playerManager.GetAllPlayers();
            foreach (var player in players)
            {
                if (player.State != PlayerState.Dead && player.Hp < player.MaxHp)
                {
                    player.Heal(2); // Regenera 2 HP a cada 2 segundos
                    
                    _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
                    {
                        Id = player.Id.ToString(),
                        Hp = player.Hp,
                        MaxHp = player.MaxHp,
                        Level = player.Level,
                        Experience = player.Experience
                    });
                }
            }
        }

        public void ProcessMonsterRespawn()
        {
            var now = DateTime.UtcNow;
            var players = _playerManager.GetAllPlayers().ToList();
            if (players.Count == 0) return;

            // Remove agendamentos prontos
            _pendingRespawns.RemoveAll(t => t <= now);

            var currentMonsters = _monsterManager.GetAllMonsters().ToList();
            var radiusSq = _config.Monsters.DespawnRadius * _config.Monsters.DespawnRadius;
            var minRadius = Math.Max(_config.Map.SafeSpawnRadius + 1, _config.Monsters.MinSpawnDistance);

            // Cap global absoluto apenas para segurança computacional
            const int absoluteMax = 500;

            foreach (var player in players)
            {
                if (currentMonsters.Count >= absoluteMax) break;

                // Conta quantos monstros estão na região de alcance deste player
                var monstersNearCount = currentMonsters.Count(m =>
                {
                    var dx = m.Position.X - player.Position.X;
                    var dy = m.Position.Y - player.Position.Y;
                    return (dx * dx + dy * dy) <= radiusSq;
                });

                // Se houver menos que o alvo por player, gera o necessário para essa "região"
                if (monstersNearCount < _config.Monsters.PerPlayer)
                {
                    int needed = _config.Monsters.PerPlayer - monstersNearCount;
                    
                    var spawned = _monsterManager.SpawnMonstersNearPosition(
                        needed,
                        player.Position,
                        minRadius,
                        _config.Monsters.SpawnRadius);

                    if (spawned != null && spawned.Any())
                    {
                        foreach (var monster in spawned)
                        {
                            // Adiciona à lista local para que o próximo player no loop
                            // veja que já foram criados monstros que podem estar perto dele também
                            currentMonsters.Add(monster);

                            _worldEvents.OnMonsterSpawned(new MonsterData
                            {
                                Id = monster.Id.ToString(),
                                Name = monster.Name,
                                ObjectCode = monster.ObjectCode,
                                Position = monster.Position,
                                Hp = monster.Hp,
                                MaxHp = monster.MaxHp,
                                AttackPower = monster.AttackPower,
                                IsDead = monster.IsDead
                            });
                        }
                    }
                }
            }
        }

        private void ProcessMonsterDespawn()
        {
            var monsters = _monsterManager.GetAllMonsters().ToList();
            var players = _playerManager.GetAllPlayers().ToList();

            if (players.Count == 0)
            {
                // Se não há players, remove todos os monstros (opcional, mas eficiente)
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
                    var dx = Math.Abs(monster.Position.X - player.Position.X);
                    var dy = Math.Abs(monster.Position.Y - player.Position.Y);
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
                    // Reutiliza OnMonsterRemoved para notificar o frontend sobre a remoção
                    _worldEvents.OnMonsterRemoved(monster.Id.ToString());
                    
                    // Adiciona ao pool de respawn para manter o desafio próximo a outros players
                    _pendingRespawns.Add(DateTime.UtcNow.AddSeconds(_config.Monsters.RespawnTimeSec));
                }
            }
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

        private bool IsPlayerMoving(IPlayer player)
        {
            // Um jogador é considerado em movimento se moveu nos últimos 100ms
            // Isso permite um pequeno atraso após parar de mover antes de poder atacar
            const int movementCooldownMs = 100;
            return (DateTime.UtcNow - player.LastMoveTime).TotalMilliseconds < movementCooldownMs;
        }

        private void ProcessItemPickup(IPlayer player)
        {
            var item = _itemManager.GetItemAt(player.Position);
            if (item != null)
            {
                if (item.Type == ItemType.Potion)
                {
                    player.Heal(20);
                    _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
                    {
                        Id = player.Id.ToString(),
                        Hp = player.Hp,
                        MaxHp = player.MaxHp,
                        Level = player.Level,
                        Experience = player.Experience
                    });
                }

                _itemManager.RemoveItem(item.Id);
                _worldEvents.OnItemPickedUp(item.Id, player.Id);
            }
        }

        public void ProcessChunkLoading(IPlayer player, string connectionId)
        {
            int cx = (int)Math.Floor((double)player.Position.X / _config.Map.ChunkSize);
            int cy = (int)Math.Floor((double)player.Position.Y / _config.Map.ChunkSize);

            // Verifica chunk atual e adjacentes baseados no LoadRadius
            for (int dx = -_config.Map.LoadRadius; dx <= _config.Map.LoadRadius; dx++)
            {
                for (int dy = -_config.Map.LoadRadius; dy <= _config.Map.LoadRadius; dy++)
                {
                    var coord = new ChunkCoord(cx + dx, cy + dy);
                    
                    // Gera o chunk se ainda não existir
                    if (!_staticWorldManager.IsChunkLoaded(coord))
                    {
                        _worldGenerator.GenerateChunk(coord);
                    }

                    // Sempre envia os dados do chunk para o jogador que está se conectando
                    var chunkObjects = _staticWorldManager.GetChunkObjects(coord);
                    var chunkItems = _itemManager.GetItemsInChunk(coord);
                    
                    var chunkData = new ChunkData
                    {
                        CX = coord.CX,
                        CY = coord.CY,
                        Objects = chunkObjects.Select(obj => new MapObjectData
                        {
                            Id = obj.Id,
                            Name = obj.Name,
                            ObjectCode = obj.ObjectCode,
                            Position = obj.Position,
                            Type = obj.Type,
                            IsPassable = obj.IsPassable
                        }).ToList(),
                        Items = chunkItems.Select(item => new ItemData
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Position = item.Position,
                            Type = item.Type
                        }).ToList()
                    };

                    _worldEvents.OnChunkLoaded(connectionId, chunkData);
                }
            }
        }
    }
}

