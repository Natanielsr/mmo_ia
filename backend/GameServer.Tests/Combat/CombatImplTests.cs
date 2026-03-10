using GameServerApp.Services;

namespace GameServer.Tests.Combat;

public class CombatImplTests
{
    private readonly CombatService _service = new();

    [Fact]
    public void CombatService_Should_Reduce_HP()
    {
        var newHp = _service.Attack(100, 10);
        Assert.Equal(90, newHp);
    }

    [Fact]
    public void CombatService_Should_Not_Drop_HP_Below_Zero()
    {
        var newHp = _service.Attack(10, 50);
        Assert.Equal(0, newHp);
    }
}
