using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.Config;
using GameServerApp.Managers;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using Xunit;

namespace GameServer.Tests.Managers
{
    public class ItemManagerTests
    {
        private readonly IOptions<WorldConfig> _options = Options.Create(new WorldConfig());

        [Fact]
        public void DropItem_ShouldAddItemToManager()
        {
            // Arrange
            var itemManager = new ItemManager(_options);
            var item = new Item("1", "Potion", 0.1f, new Position(5, 5), ItemType.Potion);

            // Act
            itemManager.DropItem(item);

            // Assert
            var result = itemManager.GetItemAt(new Position(5, 5));
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
        }

        [Fact]
        public void GetItemAt_ShouldReturnNull_WhenNoItemAtPosition()
        {
            // Arrange
            var itemManager = new ItemManager(_options);

            // Act
            var result = itemManager.GetItemAt(new Position(10, 10));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void RemoveItem_ShouldRemoveItemFromManager()
        {
            // Arrange
            var itemManager = new ItemManager(_options);
            var item = new Item("1", "Potion", 0.1f, new Position(5, 5), ItemType.Potion);
            itemManager.DropItem(item);

            // Act
            itemManager.RemoveItem("1");

            // Assert
            var result = itemManager.GetItemAt(new Position(5, 5));
            Assert.Null(result);
        }

        [Fact]
        public void GetAllItems_ShouldReturnAllDroppedItems()
        {
            // Arrange
            var itemManager = new ItemManager(_options);
            itemManager.DropItem(new Item("1", "P1", 0.1f, new Position(1, 1)));
            itemManager.DropItem(new Item("2", "P2", 0.1f, new Position(2, 2)));

            // Act
            var allItems = itemManager.GetAllItems();

            // Assert
            Assert.Equal(2, allItems.Count);
        }
    }
}
