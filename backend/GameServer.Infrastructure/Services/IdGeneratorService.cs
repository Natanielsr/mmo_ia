using GameServerApp.Contracts.Services;
using IdGen;

namespace GameServer.Infrastructure.Services;

public class IdGeneratorService : IIdGeneratorService
{
    // Configure o ID da máquina/instância (0 a 1023)
    private int generatorId;

    public IdGeneratorService(int generatorId = 1)
    {
        this.generatorId = generatorId;
    }
    public long GenerateId()
    {
        var generator = new IdGenerator(generatorId);
        return generator.CreateId();
    }
}
