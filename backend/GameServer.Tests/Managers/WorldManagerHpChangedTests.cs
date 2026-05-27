using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using GameServerApp.World;
using GameServer.Tests.Helpers;
using Moq;

namespace GameServer.Tests.Managers
{
    public class WorldManagerHpChangedTests
    {
        private readonly WorldManagerBuilder _b = new WorldManagerBuilder().WithRealServices();

        [Fact]
        public void ProcessUseItem_Fires_HpChanged_Not_StatusUpdated()
        {
            var player = new Player(1, "Hero", new Position(0, 0), maxHp: 100);
            player.TakeDamage(40);
            var potion = new HealingPotion("p1", "Healing Potion", 0.1f, new Position(0, 0));
            var inv = new PlayerInventory();
            inv.AddItem(potion);
            _b.InventoryManager.Setup(m => m.GetInventory(1L)).Returns(inv);

            _b.Build().ProcessUseItem(player, "p1");

            _b.Events.Verify(e => e.OnPlayerHpChanged(It.Is<PlayerHpData>(d => d.Id == "1")), Times.Once);
            _b.Events.Verify(e => e.OnPlayerStatusUpdated(It.IsAny<PlayerStatusData>()), Times.Never);
        }

        [Fact]
        public void Tick_Regen_Fires_HpChanged_When_Player_Below_MaxHp()
        {
            var player = new Player(1, "Hero", new Position(0, 0), maxHp: 100);
            player.TakeDamage(10);
            _b.PlayerManager.Setup(m => m.GetAllPlayers()).Returns([player]);
            _b.MonsterManager.Setup(m => m.GetAllMonsters()).Returns([]);

            var wm = _b.Build();
            var field = typeof(GameServerApp.Processors.PlayerRegenerationProcessor)
                .GetField("_lastRegenTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field!.SetValue(_b.BuiltRegeneration, System.DateTime.UtcNow.AddSeconds(-3));

            wm.Tick();

            _b.Events.Verify(e => e.OnPlayerHpChanged(It.Is<PlayerHpData>(d => d.Id == "1")), Times.Once);
            _b.Events.Verify(e => e.OnPlayerStatusUpdated(It.IsAny<PlayerStatusData>()), Times.Never);
        }

        [Fact]
        public void Tick_MonsterCombat_Fires_HpChanged_When_Monster_Attacks_Player()
        {
            var player = new Player(1, "Hero", new Position(0, 0), maxHp: 100);
            var monster = new Monster(99, "Goblin", "goblin", new Position(1, 0), maxHp: 20, attackPower: 5);
            _b.PlayerManager.Setup(m => m.GetAllPlayers()).Returns([player]);
            _b.MonsterManager.Setup(m => m.GetAllMonsters()).Returns([monster]);

            monster.LastAttackTime = System.DateTime.UtcNow.AddSeconds(-10);

            var wm = _b.Build();
            wm.Tick();

            _b.Events.Verify(e => e.OnPlayerHpChanged(It.Is<PlayerHpData>(d => d.Id == "1")), Times.Once);
        }

        [Fact]
        public void KillMonster_Fires_StatusUpdated_With_AttackPower_And_Defense()
        {
            var b = new WorldManagerBuilder().WithRealServices();
            var player = new Player(1, "Hero", new Position(0, 0), attackPoints: 10);
            var monster = new Monster(99, "Goblin", "goblin", new Position(1, 0), maxHp: 1, attackPower: 5);
            b.MonsterManager.Setup(m => m.GetMonsterById(99L)).Returns(monster);
            b.LootTable.Setup(l => l.RollLoot(It.IsAny<Position>(), It.IsAny<string>())).Returns((GameServerApp.Contracts.World.IItem?)null);

            b.Build().ProcessPlayerAttackMonster(player, "99");

            b.Events.Verify(e => e.OnPlayerStatusUpdated(It.Is<PlayerStatusData>(d =>
                d.Id == "1" && d.AttackPower == player.TotalAttackPower)), Times.Once);
            b.Events.Verify(e => e.OnPlayerHpChanged(It.IsAny<PlayerHpData>()), Times.Never);
        }
    }
}
