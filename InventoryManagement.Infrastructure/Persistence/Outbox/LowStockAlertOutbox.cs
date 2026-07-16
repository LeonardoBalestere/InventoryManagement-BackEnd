using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using System.Text.Json;

namespace InventoryManagement.Infrastructure.Persistence.Outbox;

internal sealed class LowStockAlertOutbox : ILowStockAlertOutbox
{
    private readonly ApplicationDbContext _context;

    public LowStockAlertOutbox(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnqueueAsync(LowStockAlert alert, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(LowStockAlert),
            Content = JsonSerializer.Serialize(alert),
            OccurredOnUtc = DateTime.UtcNow
        };

        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
