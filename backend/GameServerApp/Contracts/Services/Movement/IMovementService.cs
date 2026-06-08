using GameServerApp.Contracts.World;
namespace GameServerApp.Contracts.Services.Movement
{
    using Types;

    public interface IMovementService
    {
        Position Move(Position position, string direction);
    }
}
