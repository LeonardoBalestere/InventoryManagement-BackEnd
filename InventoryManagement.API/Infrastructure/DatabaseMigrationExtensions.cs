using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Infrastructure;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            if (!await context.Products.AnyAsync())
            {
                logger.LogInformation("Seeding initial data...");

                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var electronicsCategory = new Category("Electronics", "Electronic devices and accessories");
                    var peripheralsCategory = new Category("Peripherals", "Computer peripherals");

                    context.Set<Category>().AddRange(electronicsCategory, peripheralsCategory);

                    var product1 = new Product(peripheralsCategory.Id, "SKU-100", "Wireless Mouse", "A fast wireless mouse", 49.99m, 10);
                    var product2 = new Product(peripheralsCategory.Id, "SKU-101", "Mechanical Keyboard", "Clicky keyboard", 89.99m, 5);

                    product1.AddMovement(50, InventoryManagement.Domain.Enums.MovementType.Inbound, "Initial stock");
                    product2.AddMovement(20, InventoryManagement.Domain.Enums.MovementType.Inbound, "Initial stock");

                    context.Products.AddRange(product1, product2);

                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    logger.LogInformation("Initial data seeded successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Transaction rolled back. Failed to seed initial data.");
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}