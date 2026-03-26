using InventoryManagement.Application.Categories.Queries.GetCategories;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence.Repositories;

public class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CategoryDto>> GetCategoriesAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{searchTerm}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var dbItems = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = dbItems.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description
        )).ToList();

        return new PaginatedList<CategoryDto>(items, totalCount, pageNumber, pageSize);
    }
}
