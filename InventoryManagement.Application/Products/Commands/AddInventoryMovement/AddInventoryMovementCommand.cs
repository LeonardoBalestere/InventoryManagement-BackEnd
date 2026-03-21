using InventoryManagement.Application.Common.Interfaces;

namespace InventoryManagement.Application.Products.Commands.AddInventoryMovement;

public record AddInventoryMovementCommand(
    Guid ProductId,
    string MovementType,
    int Quantity,
    string? Justification,
    string IdempotencyKey 
) : MediatR.IRequest<Guid>, IIdempotentRequest;