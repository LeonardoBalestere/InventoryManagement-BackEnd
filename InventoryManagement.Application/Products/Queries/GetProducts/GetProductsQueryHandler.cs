using System.Threading;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using MediatR;

namespace InventoryManagement.Application.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetProductsQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetProductsAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}