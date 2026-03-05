using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;

namespace GameServer.Tests.DB
{
    public class PersistenceServiceTests
    {
        [Fact]
        public void SavePlayerState_Should_Be_Called_With_Player()
        {
            var mock = new Mock<IPersistenceService>();
            var playerMock = new Mock<IPlayer>();

            mock.Object.SavePlayerState(playerMock.Object);

            mock.Verify(p => p.SavePlayerState(playerMock.Object), Times.Once);
        }

        [Fact]
        public void LoadPlayerState_Should_Return_Player()
        {
            var mock = new Mock<IPersistenceService>();
            var playerMock = new Mock<IPlayer>();
            mock.Setup(p => p.LoadPlayerState("uid")).Returns(playerMock.Object);

            var result = mock.Object.LoadPlayerState("uid");
            Assert.Equal(playerMock.Object, result);
        }
    }
}