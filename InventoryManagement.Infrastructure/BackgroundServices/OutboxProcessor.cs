using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace InventoryManagement.Infrastructure.BackgroundServices;

/// <summary>
/// Polls the transactional outbox and dispatches pending messages without blocking
/// the main HTTP request thread (ADR-9).
/// </summary>
// Timing-driven hosting loop; the dispatch logic itself is covered via OutboxDispatcher tests
[ExcludeFromCodeCoverage]
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _pollingInterval = TimeSpan.FromSeconds(
            int.TryParse(configuration["Outbox:PollingIntervalSeconds"], out var seconds) && seconds > 0 ? seconds : 30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started. Polling interval: {Interval}s.", _pollingInterval.TotalSeconds);

        using var timer = new PeriodicTimer(_pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
                await dispatcher.ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing cycle failed.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
