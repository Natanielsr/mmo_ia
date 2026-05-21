namespace GameServerApp.Contracts.Processors
{
    public interface IMonsterLifecycleProcessor
    {
        void ProcessMonsterMovement();
        void ProcessMonsterRespawn();
        void ProcessMonsterDespawn();
    }
}
