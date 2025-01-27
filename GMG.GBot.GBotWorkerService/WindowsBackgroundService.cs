using GMG.GBot.Library;

namespace GMG.GBot.GBotWorkerService
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly GBotService _botService;

        public WindowsBackgroundService(GBotService botService) => (_botService) = (botService);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _botService.Start(stoppingToken);
            }
        }
    }
}