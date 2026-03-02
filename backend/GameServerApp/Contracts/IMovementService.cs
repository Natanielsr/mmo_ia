namespace GameServerApp.Contracts
{
    using Types;

    public interface IMovementService
    {
        Position Move(Position position, string direction);
    }
}
