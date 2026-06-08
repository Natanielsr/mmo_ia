using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using IdGen;

namespace GameServer.Infrastructure.Services;

public class IdGeneratorService : IIdGeneratorService
{
    // Configure o ID da máquina/instância (0 a 1023)
    private readonly int _generatorId;
    private readonly IdGenerator _generator;

    public IdGeneratorService(int generatorId = 1)
    {
        _generatorId = generatorId;
        // Cria o gerador uma vez no construtor
        _generator = new IdGenerator(_generatorId);
    }

    public long GenerateId()
    {
        // Usa a mesma instância do gerador para garantir IDs únicos
        return _generator.CreateId();
    }
}
