namespace GameServerApp.Contracts.World
{
    /// <summary>
    /// Marker interface for dynamic world objects (players, NPCs, items, etc.)
    /// These objects can move and are registered in CollisionManager.
    /// Static objects (terrain, buildings) should not implement this interface.
    /// </summary>
    public interface IDynamicWorldObject : IWorldObject
    {
        // No additional members; serves as a type discriminator
    }
}