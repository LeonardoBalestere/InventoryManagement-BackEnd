using InventoryManagement.Application.Common.Models;

namespace InventoryManagement.Application.Common.Interfaces;

public interface ICategoryReadRepository
{
    Task<PaginatedList<Categories.Queries.GetCategories.CategoryDto>> GetCategoriesAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
