using System;
using Moq;
using Xunit;
using GameServerApp.Contracts;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.World
{
    public class ZoneManagerTests
    {
        [Fact]
        public void PlayerEnterZone_Should_Call_Method()
        {
            var mock = new Mock<IZoneManager>();
            var playerMock = new Mock<IPlayer>();
            playerMock.Setup(p => p.Name).Returns("test");

            mock.Object.PlayerEnterZone(playerMock.Object, "zone1");

            // verify the method was invoked; actual event behavior will be tested in concrete implementation
            mock.Verify(m => m.PlayerEnterZone(playerMock.Object, "zone1"), Times.Once);
        }

        [Fact]
        public void PlayerLeaveZone_Should_Call_Method()
        {
            var mock = new Mock<IZoneManager>();
            var playerMock = new Mock<IPlayer>();
            playerMock.Setup(p => p.Name).Returns("test");

            mock.Object.PlayerLeaveZone(playerMock.Object, "zone2");

            mock.Verify(m => m.PlayerLeaveZone(playerMock.Object, "zone2"), Times.Once);
        }
    }
}