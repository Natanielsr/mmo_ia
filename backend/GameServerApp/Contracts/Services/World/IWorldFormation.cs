namespace GameServerApp.Contracts.Services.World
{
    public interface IWorldFormation
    {
        void Generate(int startX, int startY, int size, Random rng, Action<int, int, string?> spawnAction);
    }
}
