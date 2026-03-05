using GameServerApp.Contracts.World;
namespace GameServerApp.Contracts.Services
{
    using Types;

    public interface IMovementService
    {
        Position Move(Position position, string direction);
    }
}
