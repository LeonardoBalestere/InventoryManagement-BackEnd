using InventoryManagement.Application.Common.Models;
using MediatR;

namespace InventoryManagement.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<ProductDto>>;