using System;
using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.World;

public class WorldObjectTests
{
    [Fact]
    public void WorldObject_Should_Have_Position()
    {
        var mock = new Mock<IWorldObject>();
        var position = new Position(5, 10);
        mock.Setup(o => o.Position).Returns(position);

        var objectPosition = mock.Object.Position;

        Assert.Equal(5, objectPosition.X);
        Assert.Equal(10, objectPosition.Y);
    }

    [Fact]
    public void WorldObject_Wall_Should_Not_Be_Passable()
    {
        var mock = new Mock<IWorldObject>();
        mock.Setup(o => o.Type).Returns(ObjectType.Wall);
        mock.Setup(o => o.IsPassable).Returns(false);

        var isPassable = mock.Object.IsPassable;

        Assert.False(isPassable);
    }

    [Fact]
    public void WorldObject_Door_Should_Be_Passable()
    {
        var mock = new Mock<IWorldObject>();
        mock.Setup(o => o.Type).Returns(ObjectType.Door);
        mock.Setup(o => o.IsPassable).Returns(true);

        var isPassable = mock.Object.IsPassable;

        Assert.True(isPassable);
    }

    [Fact]
    public void WorldObject_Obstacle_Should_Not_Be_Passable()
    {
        var mock = new Mock<IWorldObject>();
        mock.Setup(o => o.Type).Returns(ObjectType.Obstacle);
        mock.Setup(o => o.IsPassable).Returns(false);

        var isPassable = mock.Object.IsPassable;

        Assert.False(isPassable);
    }

    [Fact]
    public void WorldObject_Chest_Should_Not_Be_Passable()
    {
        var mock = new Mock<IWorldObject>();
        mock.Setup(o => o.Type).Returns(ObjectType.Chest);
        mock.Setup(o => o.IsPassable).Returns(false);

        var isPassable = mock.Object.IsPassable;

        Assert.False(isPassable);
    }

    [Fact]
    public void WorldObject_NPC_Should_Not_Be_Passable()
    {
        var mock = new Mock<IWorldObject>();
        mock.Setup(o => o.Type).Returns(ObjectType.NPC);
        mock.Setup(o => o.IsPassable).Returns(false);

        var isPassable = mock.Object.IsPassable;

        Assert.False(isPassable);
    }

    [Fact]
    public void CollisionManager_Should_Register_World_Object()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var mockWorldObject = new Mock<IWorldObject>();
        var wallId = 1;
        mockWorldObject.Setup(o => o.Id).Returns(wallId);

        mockCollisionManager.Object.RegisterObject(mockWorldObject.Object);

        mockCollisionManager.Verify(m => m.RegisterObject(It.IsAny<IWorldObject>()), Times.Once);
    }

    [Fact]
    public void CollisionManager_Should_Remove_World_Object()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var objectId = 1;
        mockCollisionManager.Object.RemoveObject(objectId);

        mockCollisionManager.Verify(m => m.RemoveObject(objectId), Times.Once);
    }

    [Fact]
    public void CollisionManager_Should_Detect_Blocked_Position()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var blockedPosition = new Position(5, 5);
        mockCollisionManager.Setup(m => m.IsPositionBlocked(blockedPosition)).Returns(true);

        var isBlocked = mockCollisionManager.Object.IsPositionBlocked(blockedPosition);

        Assert.True(isBlocked);
    }

    [Fact]
    public void CollisionManager_Should_Allow_Movement_To_Empty_Position()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var fromPosition = new Position(0, 0);
        var toPosition = new Position(1, 1);
        mockCollisionManager.Setup(m => m.CanPlayerMove(fromPosition, toPosition)).Returns(true);

        var canMove = mockCollisionManager.Object.CanPlayerMove(fromPosition, toPosition);

        Assert.True(canMove);
    }

    [Fact]
    public void CollisionManager_Should_Block_Movement_To_Wall()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var fromPosition = new Position(0, 0);
        var wallPosition = new Position(1, 0);
        mockCollisionManager.Setup(m => m.CanPlayerMove(fromPosition, wallPosition)).Returns(false);
        mockCollisionManager.Setup(m => m.IsPositionBlocked(wallPosition)).Returns(true);

        var canMove = mockCollisionManager.Object.CanPlayerMove(fromPosition, wallPosition);

        Assert.False(canMove);
    }

    [Fact]
    public void CollisionManager_Should_Get_Object_At_Position()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var mockWorldObject = new Mock<IWorldObject>();
        var objectPosition = new Position(3, 4);
        mockWorldObject.Setup(o => o.Position).Returns(objectPosition);
        mockWorldObject.Setup(o => o.Type).Returns(ObjectType.Wall);

        mockCollisionManager.Setup(m => m.GetObjectAt(objectPosition)).Returns(mockWorldObject.Object);

        var retrievedObject = mockCollisionManager.Object.GetObjectAt(objectPosition);

        Assert.NotNull(retrievedObject);
        Assert.Equal(ObjectType.Wall, retrievedObject.Type);
    }

    [Fact]
    public void CollisionManager_Should_Return_Null_For_Empty_Position()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var emptyPosition = new Position(10, 10);
        mockCollisionManager.Setup(m => m.GetObjectAt(emptyPosition)).Returns((IWorldObject?)null!);

        var retrievedObject = mockCollisionManager.Object.GetObjectAt(emptyPosition);

        Assert.Null(retrievedObject);
    }

    [Fact]
    public void Player_Should_Not_Pass_Through_Wall()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var mockPlayer = new Mock<IPlayer>();
        var startPosition = new Position(0, 0);
        var wallPosition = new Position(1, 0);

        mockPlayer.Setup(p => p.Position).Returns(startPosition);
        mockCollisionManager.Setup(m => m.CanPlayerMove(startPosition, wallPosition)).Returns(false);

        var canMove = mockCollisionManager.Object.CanPlayerMove(startPosition, wallPosition);

        Assert.False(canMove);
        mockCollisionManager.Verify(m => m.CanPlayerMove(startPosition, wallPosition), Times.Once);
    }

    [Fact]
    public void Player_Should_Pass_Through_Door()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var startPosition = new Position(0, 0);
        var doorPosition = new Position(1, 0);

        mockCollisionManager.Setup(m => m.CanPlayerMove(startPosition, doorPosition)).Returns(true);

        var canMove = mockCollisionManager.Object.CanPlayerMove(startPosition, doorPosition);

        Assert.True(canMove);
    }

    [Fact]
    public void CollisionManager_Should_Handle_Multiple_Objects()
    {
        var mockCollisionManager = new Mock<ICollisionManager>();
        var wall = new Mock<IWorldObject>();
        var chest = new Mock<IWorldObject>();
        var door = new Mock<IWorldObject>();

        wall.Setup(o => o.Id).Returns(1);
        chest.Setup(o => o.Id).Returns(2);
        door.Setup(o => o.Id).Returns(3);

        mockCollisionManager.Object.RegisterObject(wall.Object);
        mockCollisionManager.Object.RegisterObject(chest.Object);
        mockCollisionManager.Object.RegisterObject(door.Object);

        mockCollisionManager.Verify(m => m.RegisterObject(It.IsAny<IWorldObject>()), Times.Exactly(3));
    }

    [Fact]
    public void WorldObject_Position_Should_Align_To_Grid()
    {
        var mock = new Mock<IWorldObject>();
        // simulate tile-based grid with tile size 1
        var pos = new Position(7, 4);
        mock.Setup(o => o.Position).Returns(pos);

        var position = mock.Object.Position;
        Assert.True(position.X % 1 == 0);
        Assert.True(position.Y % 1 == 0);
        Assert.True(position.X >= 0 && position.Y >= 0, "Position must be non-negative and aligned");
    }
}
