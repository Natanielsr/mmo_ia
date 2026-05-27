using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Processors;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Processors
{
    public class EquipmentProcessorTests
    {
        private readonly Mock<IEquipmentManager> _equipmentManager = new();
        private readonly Mock<IInventoryManager> _inventoryManager = new();
        private readonly Mock<IWorldEvents> _worldEvents = new();
        private readonly EquipmentProcessor _sut;

        public EquipmentProcessorTests()
        {
            _sut = new EquipmentProcessor(_equipmentManager.Object, _inventoryManager.Object, _worldEvents.Object);
        }

        private static Player DeadPlayer()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            player.TakeDamage(1000);
            return player;
        }

        [Fact]
        public void ProcessUnequipItem_Should_ReturnFalse_WhenPlayerIsDead()
        {
            var player = DeadPlayer();

            var result = _sut.ProcessUnequipItem(player, EquipmentSlot.Weapon);

            Assert.False(result);
            _equipmentManager.Verify(m => m.GetEquipment(It.IsAny<long>()), Times.Never);
            _worldEvents.Verify(e => e.OnEquipmentUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyDictionary<EquipmentSlot, IItem?>>()), Times.Never);
        }

        [Fact]
        public void ProcessMoveItemInInventory_Should_ReturnFalse_WhenPlayerIsDead()
        {
            var player = DeadPlayer();

            var result = _sut.ProcessMoveItemInInventory(player, "sword_01", 2);

            Assert.False(result);
            _inventoryManager.Verify(m => m.GetInventory(It.IsAny<long>()), Times.Never);
            _worldEvents.Verify(e => e.OnInventoryUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Never);
        }
    }
}
