using System;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World;

/// <summary>
/// Representa um objeto estático do mundo (árvores, pedras, muros, etc.)
/// Objetos estáticos não se movem e geralmente são carregados do mapa
/// </summary>
public class StaticObject : IWorldObject
{
    public long Id { get; }
    public Position Position { get; }
    public Size Size { get; } = new Size { Width = 1, Height = 1 };
    public ObjectType Type => ObjectType.Obstacle;
    public bool IsPassable { get; }
    public float Rotation { get; } = 0f;
    public string Name { get; }
    public string Sprite { get; }
    public bool IsInteractive { get; }

    public StaticObject(long id, Position position, string name, string sprite, bool isPassable = false, bool isInteractive = false)
    {
        Id = id;
        Position = position;
        Name = name;
        Sprite = sprite;
        IsPassable = isPassable;
        IsInteractive = isInteractive;
    }
}
