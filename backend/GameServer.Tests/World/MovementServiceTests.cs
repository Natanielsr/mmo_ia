using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.World;

public class MovementServiceTests
{
    [Fact]
    public void MovementService_Should_Change_Position()
    {
        var mock = new Mock<IMovementService>();
        mock.Setup(m => m.Move(It.Is<Position>(p => p.X == 0 && p.Y == 0), "east"))
            .Returns(new Position(1, 0));

        var result = mock.Object.Move(new Position(0, 0), "east");
        Assert.Equal(new Position(1, 0), result);
        mock.Verify(m => m.Move(It.Is<Position>(p => p.X == 0 && p.Y == 0), "east"), Times.Once);
    }
}
