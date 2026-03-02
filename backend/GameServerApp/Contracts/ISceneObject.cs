namespace GameServerApp.Contracts
{
    public interface ISceneObject
    {
        Types.Position Position { get; }
        Types.Size Size { get; }
        float Rotation { get; }
    }
}
