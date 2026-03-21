using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}