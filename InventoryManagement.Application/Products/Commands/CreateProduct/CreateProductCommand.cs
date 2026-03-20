namespace InventoryManagement.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Sku,
    string Name,
    string Description,
    decimal BasePrice,
    int MinStockLevel
) : MediatR.IRequest<Guid>;