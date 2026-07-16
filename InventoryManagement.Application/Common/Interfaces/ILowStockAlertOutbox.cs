using InventoryManagement.Application.Common.Models;

namespace InventoryManagement.Application.Common.Interfaces;

/// <summary>
/// Enqueues a low-stock alert in the transactional outbox (ADR-9). The message is persisted
/// atomically with the business change by the same <see cref="IUnitOfWork"/> commit.
/// </summary>
public interface ILowStockAlertOutbox
{
    Task EnqueueAsync(LowStockAlert alert, CancellationToken cancellationToken);
}
