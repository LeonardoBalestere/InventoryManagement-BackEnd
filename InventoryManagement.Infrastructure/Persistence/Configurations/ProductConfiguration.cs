using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Sku)
               .IsUnique();

        builder.Property(x => x.Sku)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.BasePrice)
               .HasPrecision(18, 2)
               .IsRequired();

        // Relationship
        builder.HasMany(x => x.Movements)
               .WithOne() // ProductId is a FK below
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // If we want the property backed by private field cleanly
        builder.Metadata.FindNavigation(nameof(Product.Movements))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}