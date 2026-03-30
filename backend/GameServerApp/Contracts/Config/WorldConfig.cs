namespace GameServerApp.Contracts.Config;

public class WorldConfig
{
    public int WorldWidth { get; set; } = 32;
    public int WorldHeight { get; set; } = 32;
    public int MaxMonsters { get; set; } = 15;
    public int SafeSpawnRadius { get; set; } = 4;
    public double RespawnTimeSec { get; set; } = 10.0;
    public double AttackSpeedSeconds { get; set; } = 0.5;

    // Chunks configuration
    public int ChunkSize { get; set; } = 16;
    public int LoadRadius { get; set; } = 1; // Number of adjacent chunks to load
}
