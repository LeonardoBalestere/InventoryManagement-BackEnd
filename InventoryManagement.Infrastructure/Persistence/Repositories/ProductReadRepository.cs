using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using InventoryManagement.Application.Products.Queries.GetProducts;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly ApplicationDbContext _context;

    public ProductReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductDto>> GetProductsAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Movements).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%") || EF.Functions.Like(p.Sku, $"%{searchTerm}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var dbItems = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = dbItems.Select(p => new ProductDto(
                p.Id,
                p.Sku,
                p.Name,
                p.Description,
                p.BasePrice,
                p.MinStockLevel,
                p.IsActive,
                p.GetCurrentStock()
            )).ToList();

        return new PaginatedList<ProductDto>(items, totalCount, pageNumber, pageSize);
    }
}