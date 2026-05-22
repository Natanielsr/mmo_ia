namespace GameServerApp.Contracts.World
{
    public interface IDefensiveItem : IItem
    {
        int DefenseBonus { get; }
    }
}
