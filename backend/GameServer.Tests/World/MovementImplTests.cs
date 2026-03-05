using Xunit;
using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class MovementImplTests
{
    private readonly MovementService _service = new();

    [Fact]
    public void MovementService_Should_Move_East()
    {
        var result = _service.Move(new Position(0, 0), "east");
        Assert.Equal(new Position(1, 0), result);
    }

    [Fact]
    public void MovementService_Should_Move_West()
    {
        var result = _service.Move(new Position(1, 0), "west");
        Assert.Equal(new Position(0, 0), result);
    }

    [Fact]
    public void MovementService_Should_Move_North()
    {
        var result = _service.Move(new Position(0, 0), "north");
        Assert.Equal(new Position(0, 1), result);
    }

    [Fact]
    public void MovementService_Should_Move_South()
    {
        var result = _service.Move(new Position(0, 0), "south");
        Assert.Equal(new Position(0, -1), result);
    }

    [Fact]
    public void MovementService_Should_Be_Case_Insensitive()
    {
        var result = _service.Move(new Position(0, 0), "EAST");
        Assert.Equal(new Position(1, 0), result);
    }

    [Fact]
    public void MovementService_Should_Throw_On_Invalid_Direction()
    {
        Assert.Throws<System.ArgumentException>(() =>
            _service.Move(new Position(0, 0), "diagonal"));
    }
}
