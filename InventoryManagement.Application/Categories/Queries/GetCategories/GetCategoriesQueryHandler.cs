using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using MediatR;

namespace InventoryManagement.Application.Categories.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PaginatedList<CategoryDto>>
{
    private readonly ICategoryReadRepository _readRepository;

    public GetCategoriesQueryHandler(ICategoryReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<PaginatedList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetCategoriesAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
