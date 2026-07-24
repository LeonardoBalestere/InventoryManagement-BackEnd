using System;
using FluentValidation.TestHelper;
using InventoryManagement.Application.Products.Commands.AddInventoryMovement;
using Xunit;

namespace InventoryManagement.UnitTests.Application;

[Trait("Category", "Unit")]
public class AddInventoryMovementCommandValidatorTests
{
    private readonly AddInventoryMovementCommandValidator _validator = new();

    private static AddInventoryMovementCommand CreateCommand(
        string movementType = "Inbound",
        int quantity = 10,
        string? justification = null,
        string? idempotencyKey = null)
    {
        return new AddInventoryMovementCommand(
            Guid.NewGuid(),
            movementType,
            quantity,
            justification,
            idempotencyKey ?? Guid.NewGuid().ToString());
    }

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(CreateCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("not-a-guid")]
    [InlineData("d2719c5e-XXXX-4d55-a5a9-000000000000")]
    public void Validate_IdempotencyKeyIsNotAGuid_HasError(string invalidKey)
    {
        var result = _validator.TestValidate(CreateCommand(idempotencyKey: invalidKey));

        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void Validate_EmptyIdempotencyKey_HasError()
    {
        var result = _validator.TestValidate(CreateCommand(idempotencyKey: string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void Validate_InvalidMovementType_HasError()
    {
        var result = _validator.TestValidate(CreateCommand(movementType: "Sideways"));

        result.ShouldHaveValidationErrorFor(x => x.MovementType);
    }

    [Fact]
    public void Validate_ZeroQuantity_HasError()
    {
        var result = _validator.TestValidate(CreateCommand(quantity: 0));

        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_AdjustmentWithoutJustification_HasError()
    {
        var result = _validator.TestValidate(CreateCommand(movementType: "Adjustment", justification: null));

        result.ShouldHaveValidationErrorFor(x => x.Justification);
    }
}
