using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using InventoryManagement.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryManagement.Infrastructure.BackgroundServices;

public sealed class OutboxDispatcher
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;

    private readonly IOutboxMessageRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IOutboxMessageRepository repository, IEmailSender emailSender, ILogger<OutboxDispatcher> logger)
    {
        _repository = repository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var messages = await _repository.GetUnprocessedAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            message.AttemptCount++;

            try
            {
                await DispatchAsync(message, cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to dispatch outbox message {MessageId} (attempt {Attempt}).", message.Id, message.AttemptCount);

                if (message.AttemptCount >= MaxAttempts)
                {
                    // Dead-letter: stop retrying but keep the row and its error for auditing
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    _logger.LogError("Outbox message {MessageId} exceeded {MaxAttempts} attempts and was dead-lettered.", message.Id, MaxAttempts);
                }
            }
        }

        if (messages.Count > 0)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }

    private Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case nameof(LowStockAlert):
                var alert = JsonSerializer.Deserialize<LowStockAlert>(message.Content)
                    ?? throw new InvalidOperationException($"Outbox message {message.Id} has an empty payload.");

                var subject = $"Low stock alert: {alert.Name} ({alert.Sku})";
                var body =
                    $"<h2>Low stock alert</h2>" +
                    $"<p>Product <strong>{alert.Name}</strong> (SKU: {alert.Sku}) dropped below its minimum stock level.</p>" +
                    $"<ul><li>Current stock: {alert.CurrentStock}</li><li>Minimum stock level: {alert.MinStockLevel}</li></ul>";

                return _emailSender.SendAsync(subject, body, cancellationToken);

            default:
                throw new InvalidOperationException($"Unknown outbox message type '{message.Type}'.");
        }
    }
}
