// backend/GameServerApp/Managers/WorldProcessor.cs
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using GameServerApp.World;

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

        public WorldManager(
            IMovementService movementService,
            ICollisionManager collisionManager,
            ICombatService combatService,
            IGameStateManager gameStateManager,
            IWorldEvents worldEvents,
            IStaticWorldManager staticWorldManager,
            IMonsterMovementService monsterMovementService,
            IMonsterManager monsterManager,
            IPlayerManager playerManager)
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

                _worldEvents.OnPlayerMoved(new PlayerPositionData() { Id = player.Id, Name = player.Name, Position = targetPos, Hp = player.Hp, MaxHp = player.MaxHp, IsDead = player.IsDead });
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
            // Processa movimento dos monstros
            ProcessMonsterMovement();

            // Processa ataques dos monstros
            ProcessMonsterCombat();

            // Respawn de monstros mortos
            ProcessMonsterRespawn();
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
                        _worldEvents.OnPlayerAttacked(monster.Id, player.Id, monster.AttackPower);

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

        private void ProcessMonsterRespawn()
        {
            // TODO: Implementar respawn de monstros mortos após um tempo
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