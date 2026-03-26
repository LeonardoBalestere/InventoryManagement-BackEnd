using InventoryManagement.Application.Common.Models;
using MediatR;

namespace InventoryManagement.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<CategoryDto>>;
