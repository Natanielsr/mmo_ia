using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Processors;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Processors
{
    public class ItemProcessorTests
    {
        private readonly Mock<IItemManager> _itemManager = new();
        private readonly Mock<IInventoryManager> _inventoryManager = new();
        private readonly Mock<IWorldEvents> _worldEvents = new();
        private readonly ItemProcessor _sut;

        public ItemProcessorTests()
        {
            _sut = new ItemProcessor(_itemManager.Object, _inventoryManager.Object, _worldEvents.Object);
        }

        private static Player DeadPlayer()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            player.TakeDamage(1000);
            return player;
        }

        [Fact]
        public void ProcessItemPickup_Should_DoNothing_WhenPlayerIsDead()
        {
            var player = DeadPlayer();

            _sut.ProcessItemPickup(player);

            _itemManager.Verify(m => m.GetItemsAt(It.IsAny<Position>()), Times.Never);
            _inventoryManager.Verify(m => m.AddItem(It.IsAny<long>(), It.IsAny<IItem>()), Times.Never);
            _worldEvents.Verify(e => e.OnItemPickedUp(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            _worldEvents.Verify(e => e.OnInventoryUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Never);
        }

        [Fact]
        public void ProcessUseItem_Should_DoNothing_WhenPlayerIsDead()
        {
            var player = DeadPlayer();

            _sut.ProcessUseItem(player, "potion_01");

            _inventoryManager.Verify(m => m.GetInventory(It.IsAny<long>()), Times.Never);
            _worldEvents.Verify(e => e.OnPlayerHpChanged(It.IsAny<GameServerApp.Dtos.PlayerHpData>()), Times.Never);
            _worldEvents.Verify(e => e.OnInventoryUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Never);
        }

        [Fact]
        public void ProcessDropItem_Should_DoNothing_WhenPlayerIsDead()
        {
            var player = DeadPlayer();

            _sut.ProcessDropItem(player, "sword_01", new Position(1, 1));

            _inventoryManager.Verify(m => m.GetInventory(It.IsAny<long>()), Times.Never);
            _itemManager.Verify(m => m.DropItem(It.IsAny<IItem>()), Times.Never);
            _worldEvents.Verify(e => e.OnItemDropped(It.IsAny<IItem>()), Times.Never);
            _worldEvents.Verify(e => e.OnInventoryUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Never);
        }
    }
}
