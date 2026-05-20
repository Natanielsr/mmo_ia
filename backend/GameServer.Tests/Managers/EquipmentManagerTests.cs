using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    public class EquipmentManagerTests
    {
        private readonly EquipmentManager _mgr = new();
        private static Position P => new(0, 0);

        [Fact]
        public void GetOrCreate_Creates_Equipment_For_New_Player()
            => Assert.NotNull(_mgr.GetOrCreate(1L));

        [Fact]
        public void GetOrCreate_Returns_Same_Instance_For_Same_Player()
        {
            var eq1 = _mgr.GetOrCreate(1L);
            var eq2 = _mgr.GetOrCreate(1L);
            Assert.Same(eq1, eq2);
        }

        [Fact]
        public void GetEquipment_Returns_Null_For_Unknown_Player()
            => Assert.Null(_mgr.GetEquipment(99L));

        [Fact]
        public void Remove_Cleans_Up_Entry()
        {
            _mgr.GetOrCreate(1L);
            _mgr.Remove(1L);
            Assert.Null(_mgr.GetEquipment(1L));
        }

        [Fact]
        public void EquipItem_Delegates_To_PlayerEquipment()
        {
            var sword = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);
            var eq    = _mgr.GetOrCreate(1L);
            eq.Equip(sword);
            Assert.Same(sword, eq.GetItem(EquipmentSlot.Weapon));
        }

        [Fact]
        public void UnequipItem_Delegates_To_PlayerEquipment()
        {
            var sword = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);
            var eq    = _mgr.GetOrCreate(1L);
            eq.Equip(sword);

            var removed = eq.Unequip(EquipmentSlot.Weapon);

            Assert.Same(sword, removed);
            Assert.Null(eq.GetItem(EquipmentSlot.Weapon));
        }
    }
}
