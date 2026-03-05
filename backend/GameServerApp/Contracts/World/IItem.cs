using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public interface IItem
    {
        string Id { get; }
        string Name { get; }
        Position Position { get; set; } // if dropped on map
        float Weight { get; }
    }
}
