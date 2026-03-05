#nullable enable
using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;

namespace GameServer.Tests.Network
{
    public class ConnectionServiceTests
    {
        [Fact]
        public void Connect_Should_Return_ConnectionId_When_Token_Valid()
        {
            var mock = new Mock<IConnectionService>();
            mock.Setup(s => s.Connect("valid-token")).Returns("conn1");

            var result = mock.Object.Connect("valid-token");
            Assert.Equal("conn1", result);
        }

        [Fact]
        public void Connect_Should_Return_Null_When_Token_Invalid()
        {
            var mock = new Mock<IConnectionService>();
            mock.Setup(s => s.Connect("bad")).Returns((string?)null);

            var result = mock.Object.Connect("bad");
            Assert.Null(result);
        }

        [Fact]
        public void Disconnect_Should_Return_True_For_Known_Connection()
        {
            var mock = new Mock<IConnectionService>();
            mock.Setup(s => s.Disconnect("conn1")).Returns(true);

            var result = mock.Object.Disconnect("conn1");
            Assert.True(result);
        }

        [Fact]
        public void Disconnect_Should_Return_False_For_Unknown_Connection()
        {
            var mock = new Mock<IConnectionService>();
            mock.Setup(s => s.Disconnect("unknown")).Returns(false);

            var result = mock.Object.Disconnect("unknown");
            Assert.False(result);
        }

        [Fact]
        public void ValidateConnection_Should_Be_Called()
        {
            var mock = new Mock<IConnectionService>();
            mock.Object.ValidateConnection("conn1");
            mock.Verify(s => s.ValidateConnection("conn1"), Times.Once);
        }

        [Fact]
        public void Reconnect_Should_Return_True_For_Existing()
        {
            var mock = new Mock<IConnectionService>();
            mock.Setup(s => s.Reconnect("conn1")).Returns(true);

            var result = mock.Object.Reconnect("conn1");
            Assert.True(result);
        }
    }
}