namespace InventoryManagement.Application.Common.Models;

public sealed record LowStockAlert(
    Guid ProductId,
    string Sku,
    string Name,
    int CurrentStock,
    int MinStockLevel);
