using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Common.Models;

public record PaginationQuery(int PageNumber = 1, int PageSize = 10);