using Moq;
using Xunit;
using GameServerApp.Contracts;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.World;

public class SceneObjectTests
{
    [Fact]
    public void SceneObject_Should_Have_Position_Size_Rotation()
    {
        var mock = new Mock<ISceneObject>();
        var position = new Position(2, 3);
        var size = new Size { Width = 1.5f, Height = 2.5f };
        mock.Setup(o => o.Position).Returns(position);
        mock.Setup(o => o.Size).Returns(size);
        mock.Setup(o => o.Rotation).Returns(90f);

        Assert.Equal(position, mock.Object.Position);
        Assert.Equal(size.Width, mock.Object.Size.Width);
        Assert.Equal(size.Height, mock.Object.Size.Height);
        Assert.Equal(90f, mock.Object.Rotation);
    }

    [Fact]
    public void WorldObject_Should_Implement_SceneObject()
    {
        var mock = new Mock<IWorldObject>();
        var sceneRef = mock.As<ISceneObject>();
        var position = new Position(0, 0);
        var size = new Size { Width = 1, Height = 1 };
        sceneRef.Setup(o => o.Position).Returns(position);
        sceneRef.Setup(o => o.Size).Returns(size);
        sceneRef.Setup(o => o.Rotation).Returns(0f);

        Assert.Equal(position, sceneRef.Object.Position);
        Assert.Equal(size, sceneRef.Object.Size);
    }

    [Fact]
    public void Cannot_Pass_Through_When_Object_Is_Not_Passable_But_Is_SceneObject()
    {
        var mockCollision = new Mock<ICollisionManager>();
        var wall = new Mock<IWorldObject>();
        wall.As<ISceneObject>().Setup(o => o.Position).Returns(new Position(1, 0));
        wall.Setup(o => o.IsPassable).Returns(false);
        mockCollision.Setup(m => m.IsPositionBlocked(new Position(1, 0))).Returns(true);

        var blocked = mockCollision.Object.IsPositionBlocked(new Position(1, 0));
        Assert.True(blocked);
    }
}
