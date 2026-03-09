using System;

namespace GameServerApp.Contracts.World;

/// <summary>
/// Marker interface for static world objects (tree, rock etc.)
/// These objects can move and are registered in CollisionManager.
/// </summary>
public interface IStaticWorldObject : IWorldObject
{

}
