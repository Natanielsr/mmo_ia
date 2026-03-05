using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;

namespace GameServer.Tests.Combat;

public class CombatServiceTests
{
    [Fact]
    public void CombatService_Should_Reduce_HP()
    {
        var mock = new Mock<ICombatService>();
        mock.Setup(c => c.Attack(100, 10)).Returns(90);

        var newHp = mock.Object.Attack(100, 10);
        Assert.Equal(90, newHp);
        mock.Verify(c => c.Attack(100, 10), Times.Once);
    }
}
