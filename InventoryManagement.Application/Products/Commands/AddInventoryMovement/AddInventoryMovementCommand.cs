using System;

namespace InventoryManagement.Application.Products.Commands.AddInventoryMovement;

public record AddInventoryMovementCommand(
    Guid ProductId,
    string MovementType,
    int Quantity,
    string? Justification,
    string IdempotencyKey // Rule 7: Idempotency Key
) : MediatR.IRequest<Guid>;