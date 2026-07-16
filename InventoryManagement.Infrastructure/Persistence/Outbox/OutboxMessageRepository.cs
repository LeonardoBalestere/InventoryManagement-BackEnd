using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly ApplicationDbContext _context;

    public OutboxMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken)
    {
        return _context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
