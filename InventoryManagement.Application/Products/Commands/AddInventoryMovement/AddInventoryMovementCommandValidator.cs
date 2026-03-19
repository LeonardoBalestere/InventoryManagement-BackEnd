using FluentValidation;

namespace InventoryManagement.Application.Products.Commands.AddInventoryMovement;

public sealed class AddInventoryMovementCommandValidator : AbstractValidator<AddInventoryMovementCommand>
{
    public AddInventoryMovementCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.MovementType)
            .NotEmpty().WithMessage("Movement type is required.")
            .Must(t => t == "Inbound" || t == "Outbound" || t == "Adjustment")
            .WithMessage("Movement type must be Inbound, Outbound, or Adjustment.");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero for any movement.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("An Idempotency Key is required to prevent duplicate processing.");

        When(x => x.MovementType == "Adjustment", () =>
        {
            RuleFor(x => x.Justification)
                .NotEmpty().WithMessage("A valid justification is required for Adjustment movements.")
                .MaximumLength(255).WithMessage("Justification cannot exceed 255 characters.");
        });

        When(x => x.MovementType != "Adjustment" && !string.IsNullOrEmpty(x.Justification), () =>
        {
            RuleFor(x => x.Justification)
                .MaximumLength(255).WithMessage("Justification cannot exceed 255 characters.");
        });
    }
}