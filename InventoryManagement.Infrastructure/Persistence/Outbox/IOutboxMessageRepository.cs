namespace InventoryManagement.Infrastructure.Persistence.Outbox;

public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
