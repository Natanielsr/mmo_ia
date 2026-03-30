namespace GameServerApp.Contracts.Config;

public class WorldConfig
{
    public int WorldWidth { get; set; } = 32;
    public int WorldHeight { get; set; } = 32;
    public int MaxMonsters { get; set; } = 50; // Aumentado o limite global
    public int MonstersPerPlayer { get; set; } = 10;
    public int SafeSpawnRadius { get; set; } = 4;
    public int MinMonsterSpawnDistance { get; set; } = 12;
    public int SpawnMonsterRadius { get; set; } = 18;
    public int DespawnMonsterRadius { get; set; } = 30;
    public double RespawnTimeSec { get; set; } = 2.0; // Respawn bem mais rápido
    public double AttackSpeedSeconds { get; set; } = 0.5;

    // Chunks configuration
    public int ChunkSize { get; set; } = 16;
    public int LoadRadius { get; set; } = 1; // Number of adjacent chunks to load
}
