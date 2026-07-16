using InventoryManagement.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Content)
               .IsRequired();

        builder.Property(x => x.Error)
               .HasMaxLength(1000);

        // Speeds up the polling query for pending messages
        builder.HasIndex(x => x.ProcessedOnUtc);
    }
}
