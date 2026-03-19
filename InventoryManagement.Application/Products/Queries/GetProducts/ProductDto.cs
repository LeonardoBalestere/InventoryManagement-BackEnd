namespace InventoryManagement.Application.Products.Queries.GetProducts;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal BasePrice,
    int MinStockLevel,
    bool IsActive,
    int CurrentStock
);