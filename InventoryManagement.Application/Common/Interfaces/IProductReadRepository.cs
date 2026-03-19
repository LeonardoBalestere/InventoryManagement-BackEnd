using System.Threading;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Models;

namespace InventoryManagement.Application.Common.Interfaces;

public interface IProductReadRepository
{
    Task<PaginatedList<Products.Queries.GetProducts.ProductDto>> GetProductsAsync(
        string? searchTerm, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken);
}