using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;
using GameServer.Tests.Helpers;
using Moq;

namespace GameServer.Tests.Items
{
    public class MonsterDeathDropTests
    {
        // Um único builder — mocks acessíveis via _b.*
        private readonly WorldManagerBuilder _b = new();

        [Fact]
        public void Monster_Death_Drop_Uses_LootTable_Not_Hardcoded_Logic()
        {
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", new Position(1, 0), 10, 5);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(999);
            _b.LootTable     .Setup(l => l.RollLoot(It.IsAny<Position>(), It.IsAny<string>(), null))
                             .Returns((IItem?)null);

            _b.Build().ProcessPlayerAttackMonster(player, "2");

            _b.LootTable.Verify(
                l => l.RollLoot(It.IsAny<Position>(), It.IsAny<string>(), null),
                Times.Once
            );
        }

        [Fact]
        public void Monster_Death_Can_Drop_Weapon()
        {
            var pos     = new Position(1, 0);
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", pos, 10, 5);
            var weapon  = new Weapon("w1", "Iron Sword", 1.5f, pos, attackBonus: 5);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(1);
            _b.LootTable     .Setup(l => l.RollLoot(pos, It.IsAny<string>(), null)).Returns(weapon);

            _b.Build().ProcessPlayerAttackMonster(player, "2");

            _b.ItemManager.Verify(m => m.DropItem(weapon), Times.Once);
            _b.Events     .Verify(e => e.OnItemDropped(weapon), Times.Once);
        }

        [Fact]
        public void Monster_Death_Can_Drop_Armor()
        {
            var pos     = new Position(1, 0);
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", pos, 10, 5);
            var armor   = new Armor("a1", "Leather Vest", 2.0f, pos, defenseBonus: 3);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(1);
            _b.LootTable     .Setup(l => l.RollLoot(pos, It.IsAny<string>(), null)).Returns(armor);

            _b.Build().ProcessPlayerAttackMonster(player, "2");

            _b.ItemManager.Verify(m => m.DropItem(armor), Times.Once);
            _b.Events     .Verify(e => e.OnItemDropped(armor), Times.Once);
        }

        [Fact]
        public void Monster_Death_No_Drop_When_LootTable_Returns_Null()
        {
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", new Position(1, 0), 10, 5);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(1);
            _b.LootTable     .Setup(l => l.RollLoot(It.IsAny<Position>(), It.IsAny<string>(), null))
                             .Returns((IItem?)null);

            _b.Build().ProcessPlayerAttackMonster(player, "2");

            _b.ItemManager.Verify(m => m.DropItem(It.IsAny<IItem>()), Times.Never);
            _b.Events     .Verify(e => e.OnItemDropped(It.IsAny<IItem>()), Times.Never);
        }

        [Fact]
        public void ItemDropped_Event_Payload_Includes_Stats_For_Weapon()
        {
            var pos     = new Position(1, 0);
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", pos, 10, 5);
            var weapon  = new Weapon("w1", "Sword", 1f, pos, attackBonus: 10);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(1);
            _b.LootTable     .Setup(l => l.RollLoot(pos, It.IsAny<string>(), null)).Returns(weapon);

            IItem? captured = null;
            _b.Events.Setup(e => e.OnItemDropped(It.IsAny<IItem>()))
                     .Callback<IItem>(item => captured = item);

            _b.Build().ProcessPlayerAttackMonster(player, "2");

            Assert.NotNull(captured);
            var capturedWeapon = Assert.IsType<Weapon>(captured);
            Assert.Equal(10, capturedWeapon.AttackBonus);
        }
    }
}
