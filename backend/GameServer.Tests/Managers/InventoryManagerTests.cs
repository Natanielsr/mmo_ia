using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using Moq;

namespace GameServer.Tests.Managers
{
    public class InventoryManagerTests
    {
        private readonly InventoryManager _mgr = new();

        [Fact]
        public void GetOrCreate_Creates_Inventory_For_New_Player()
            => Assert.NotNull(_mgr.GetOrCreate(1L));

        [Fact]
        public void GetOrCreate_Returns_Same_Instance_For_Same_Player()
        {
            var inv1 = _mgr.GetOrCreate(1L);
            var inv2 = _mgr.GetOrCreate(1L);
            Assert.Same(inv1, inv2);
        }

        [Fact]
        public void GetInventory_Returns_Null_For_Unknown_Player()
            => Assert.Null(_mgr.GetInventory(99L));

        [Fact]
        public void Remove_Deletes_Player_Entry()
        {
            _mgr.GetOrCreate(1L);
            _mgr.Remove(1L);
            Assert.Null(_mgr.GetInventory(1L));
        }

        [Fact]
        public void AddItem_Delegates_To_PlayerInventory()
        {
            var item = new Mock<IItem>();
            item.Setup(i => i.Id).Returns("i1");

            Assert.True(_mgr.AddItem(1L, item.Object));

            Assert.Single(_mgr.GetInventory(1L)!.GetItems());
        }
    }
}
