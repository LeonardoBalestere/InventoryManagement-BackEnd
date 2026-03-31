using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Errors;

namespace InventoryManagement.Domain.Entities;

public sealed class InventoryMovement
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset MovementDate { get; private set; }
    public string? Justification { get; private set; }

    // Parameterless constructor for ORM
    private InventoryMovement() { }

    internal InventoryMovement(Guid productId, MovementType type, int quantity, string? justification)
    {
        if (productId == Guid.Empty)
            throw new DomainException(DomainErrors.InventoryMovement.ProductIdEmpty);

        if ((type == MovementType.Inbound || type == MovementType.Outbound) && quantity <= 0)
            throw new DomainException(string.Format(DomainErrors.InventoryMovement.QuantityGreaterThanZeroFormat, type));

        if (type == MovementType.Adjustment && quantity == 0)
            throw new DomainException(DomainErrors.InventoryMovement.InvalidAdjustmentZero);

        if (type == MovementType.Adjustment && string.IsNullOrWhiteSpace(justification))
            throw new DomainException(DomainErrors.InventoryMovement.InvalidAdjustment);

        if (!string.IsNullOrWhiteSpace(justification) && justification.Length > 255)
            throw new DomainException(DomainErrors.InventoryMovement.JustificationMax);

        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        MovementDate = DateTimeOffset.UtcNow;
        Justification = justification?.Trim();
    }
}