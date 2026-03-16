using GameServerApp.Contracts.Managers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Services
{
    public class GameLoopService : BackgroundService
    {
        private readonly IWorldManager _worldManager;
        private readonly ILogger<GameLoopService> _logger;
        private const int TICK_INTERVAL_MS = 100; // 10 ticks por segundo

        public GameLoopService(IWorldManager worldManager, ILogger<GameLoopService> logger)
        {
            _worldManager = worldManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Game Loop Service starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Executa o tick do mundo
                    _worldManager.Tick();

                    // Aguarda o próximo tick
                    await Task.Delay(TICK_INTERVAL_MS, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Serviço está sendo parado
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in game loop tick");
                    await Task.Delay(1000, stoppingToken); // Espera 1 segundo em caso de erro
                }
            }

            _logger.LogInformation("Game Loop Service stopping...");
        }
    }
}