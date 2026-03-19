using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
               .IsRequired();

        builder.Property(x => x.Type)
               .IsRequired()
               .HasConversion<string>(); // Keep it as string in DB

        builder.Property(x => x.Quantity)
               .IsRequired();

        builder.Property(x => x.MovementDate)
               .IsRequired();

        builder.Property(x => x.Justification)
               .HasMaxLength(255);
    }
}