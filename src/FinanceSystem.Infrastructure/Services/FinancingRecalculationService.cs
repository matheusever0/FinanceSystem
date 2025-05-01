using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceSystem.Infrastructure.Services
{
    // Crie uma nova classe em Infrastructure/Services
    public class FinancingRecalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FinancingRecalculationService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromDays(1); // Recalcular diariamente

        public FinancingRecalculationService(
            IServiceProvider serviceProvider,
            ILogger<FinancingRecalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Financing Recalculation Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Financing Recalculation Service running at: {Time}", DateTimeOffset.Now);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var financingService = scope.ServiceProvider.GetRequiredService<IFinancingService>();

                    // Obter financiamentos ativos
                    var activeFinancings = await unitOfWork.Financings.GetFinancingsByStatusAsync(
                        Guid.Empty, FinancingStatus.Active);

                    foreach (var financing in activeFinancings)
                    {
                        try
                        {
                            await financingService.RecalculateRemainingInstallmentsAsync(financing.Id);
                            _logger.LogInformation("Successfully recalculated financing {Id}", financing.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error recalculating financing {Id}", financing.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during financing recalculation");
                }

                _logger.LogInformation("Financing Recalculation Service completed. Waiting for next run.");
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }
    }
}
