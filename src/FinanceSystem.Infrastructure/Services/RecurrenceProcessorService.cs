using FinanceSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceSystem.Infrastructure.Services
{
    public class RecurrenceProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RecurrenceProcessorService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromHours(24); 

        public RecurrenceProcessorService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RecurrenceProcessorService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurrence Processor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Recurrence Processor Service running at: {Time}", DateTimeOffset.Now);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var recurrenceService = scope.ServiceProvider.GetRequiredService<IRecurrenceService>();

                    await recurrenceService.ProcessRecurringPaymentsAsync();
                    await recurrenceService.ProcessRecurringIncomesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing recurrences");
                }

                _logger.LogInformation("Recurrence Processor Service completed. Waiting for next run.");
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }
    }
}