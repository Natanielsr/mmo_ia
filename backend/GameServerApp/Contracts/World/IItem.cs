using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public enum ItemType
    {
        Generic,
        Potion
    }

    public interface IItem
    {
        string Id { get; }
        string Name { get; }
        Position Position { get; set; } // if dropped on map
        float Weight { get; }
        ItemType Type { get; }
    }
}
