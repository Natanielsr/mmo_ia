using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Processors;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Processors
{
    public class PlayerRegenerationProcessorTests
    {
        private readonly Mock<IPlayerManager> _playerManager = new();
        private readonly Mock<IWorldEvents> _worldEvents = new();
        private readonly PlayerRegenerationProcessor _sut;

        public PlayerRegenerationProcessorTests()
        {
            _sut = new PlayerRegenerationProcessor(_playerManager.Object, _worldEvents.Object);
        }

        [Fact(Skip = "Regeneração passiva desabilitada")]
        public void Should_Heal_Alive_Player_With_Low_Hp_After_2s()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            player.TakeDamage(10);
            int hpBefore = player.Hp;

            _playerManager.Setup(m => m.GetAllPlayers()).Returns([player]);

            TriggerRegenAfterDelay(_sut);

            Assert.Equal(hpBefore + 2, player.Hp);
            _worldEvents.Verify(e => e.OnPlayerHpChanged(It.Is<PlayerHpData>(d =>
                d.Id == "1" && d.Hp == player.Hp && !d.IsDead)), Times.Once);
        }

        [Fact]
        public void Should_Not_Heal_Dead_Player()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            player.TakeDamage(100);

            _playerManager.Setup(m => m.GetAllPlayers()).Returns([player]);

            TriggerRegenAfterDelay(_sut);

            Assert.Equal(PlayerState.Dead, player.State);
            _worldEvents.Verify(e => e.OnPlayerHpChanged(It.IsAny<PlayerHpData>()), Times.Never);
        }

        [Fact]
        public void Should_Not_Heal_Player_At_Full_Hp()
        {
            var player = new Player(1, "Hero", new Position(0, 0));

            _playerManager.Setup(m => m.GetAllPlayers()).Returns([player]);

            TriggerRegenAfterDelay(_sut);

            Assert.Equal(player.MaxHp, player.Hp);
            _worldEvents.Verify(e => e.OnPlayerHpChanged(It.IsAny<PlayerHpData>()), Times.Never);
        }

        [Fact]
        public void Should_Not_Heal_Before_2s_Interval()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            player.TakeDamage(10);
            int hpBefore = player.Hp;

            _playerManager.Setup(m => m.GetAllPlayers()).Returns([player]);

            // Chama imediatamente sem esperar o intervalo
            _sut.ProcessPlayerRegeneration();

            Assert.Equal(hpBefore, player.Hp);
            _worldEvents.Verify(e => e.OnPlayerHpChanged(It.IsAny<PlayerHpData>()), Times.Never);
        }

        // Cria um processor com _lastRegenTime no passado para simular passagem de 2s
        private static void TriggerRegenAfterDelay(PlayerRegenerationProcessor processor)
        {
            // Força o campo privado via reflexão para simular tempo decorrido
            var field = typeof(PlayerRegenerationProcessor)
                .GetField("_lastRegenTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            field.SetValue(processor, DateTime.UtcNow.AddSeconds(-3));

            processor.ProcessPlayerRegeneration();
        }
    }
}
