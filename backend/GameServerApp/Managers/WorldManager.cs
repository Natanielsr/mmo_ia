using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Managers
{
    public class WorldManager : IWorldManager
    {
        private readonly IPlayerMovementProcessor _movementProcessor;
        private readonly ICombatProcessor _combatProcessor;
        private readonly IMonsterLifecycleProcessor _monsterLifecycleProcessor;
        private readonly IItemProcessor _itemProcessor;
        private readonly IEquipmentProcessor _equipmentProcessor;
        private readonly IChunkProcessor _chunkProcessor;
        private readonly IPlayerRegenerationProcessor _regenerationProcessor;
        private readonly IPlayerHoTProcessor _hotProcessor;
        private readonly IPlayerManager _playerManager;
        private readonly ICollisionManager _collisionManager;
        private readonly IStaticWorldManager _staticWorldManager;

        public WorldManager(
            IPlayerMovementProcessor movementProcessor,
            ICombatProcessor combatProcessor,
            IMonsterLifecycleProcessor monsterLifecycleProcessor,
            IItemProcessor itemProcessor,
            IEquipmentProcessor equipmentProcessor,
            IChunkProcessor chunkProcessor,
            IPlayerRegenerationProcessor regenerationProcessor,
            IPlayerHoTProcessor hotProcessor,
            IPlayerManager playerManager,
            ICollisionManager collisionManager,
            IStaticWorldManager staticWorldManager)
        {
            _movementProcessor = movementProcessor;
            _combatProcessor = combatProcessor;
            _monsterLifecycleProcessor = monsterLifecycleProcessor;
            _itemProcessor = itemProcessor;
            _equipmentProcessor = equipmentProcessor;
            _chunkProcessor = chunkProcessor;
            _regenerationProcessor = regenerationProcessor;
            _hotProcessor = hotProcessor;
            _playerManager = playerManager;
            _collisionManager = collisionManager;
            _staticWorldManager = staticWorldManager;
        }

        public bool ProcessPlayerMovement(IPlayer player, string direction)
        {
            bool moved = _movementProcessor.ProcessPlayerMovement(player, direction);
            if (moved)
            {
                var connId = _playerManager.GetConnectionIdByPlayerId(player.Id);
                if (connId != null) _chunkProcessor.ProcessChunkLoading(player, connId);
                _itemProcessor.ProcessItemPickup(player);
            }
            return moved;
        }

        public void ProcessPlayerAttack(IPlayer player, IPlayer target)
            => _combatProcessor.ProcessPlayerAttack(player, target);

        public void ProcessPlayerAttackMonster(IPlayer player, string monsterId)
            => _combatProcessor.ProcessPlayerAttackMonster(player, monsterId);

        public void Tick()
        {
            _monsterLifecycleProcessor.ProcessMonsterMovement();
            _combatProcessor.ProcessMonsterCombat();
            _monsterLifecycleProcessor.ProcessMonsterDespawn();
            _monsterLifecycleProcessor.ProcessMonsterRespawn();
            _hotProcessor.ProcessHoT();
        }

        public void InstantiateObject(IWorldObject worldObject)
        {
            if (worldObject is IDynamicWorldObject dynamicWorldObject)
                _collisionManager.RegisterDynamicObject(dynamicWorldObject);
            else if (worldObject is IStaticWorldObject staticWorldObject)
                _staticWorldManager.AddStaticObject(staticWorldObject);
            else
                throw new Exception("Invalid object type must be Dynamic or Static world object");
        }

        public void ProcessChunkLoading(IPlayer player, string connectionId)
            => _chunkProcessor.ProcessChunkLoading(player, connectionId);

        public void ProcessUseItem(IPlayer player, string itemId)
            => _itemProcessor.ProcessUseItem(player, itemId);

        public void ProcessDropItem(IPlayer player, string itemId, Position targetPos)
            => _itemProcessor.ProcessDropItem(player, itemId, targetPos);

        public bool ProcessEquipItem(IPlayer player, string itemId)
            => _equipmentProcessor.ProcessEquipItem(player, itemId);

        public bool ProcessUnequipItem(IPlayer player, EquipmentSlot slot)
            => _equipmentProcessor.ProcessUnequipItem(player, slot);

        public bool ProcessMoveItemInInventory(IPlayer player, string itemId, int toIndex)
            => _equipmentProcessor.ProcessMoveItemInInventory(player, itemId, toIndex);

    }
}
