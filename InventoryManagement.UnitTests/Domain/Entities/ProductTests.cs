using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Errors;
using System;
using Xunit;

namespace InventoryManagement.UnitTests.Domain.Entities;

[Trait("Category", "Unit")]
public class ProductTests
{
    private readonly Guid _validCategoryId = Guid.NewGuid();
    private readonly string _validSku = "PROD-123";
    private readonly string _validName = "Test Product";
    private readonly string _validDesc = "A test product";
    private readonly decimal _validPrice = 10.50m;
    private readonly int _validMinStock = 5;

    [Theory]
    [InlineData(50, MovementType.Inbound, "Initial Supplier Delivery", 50)]
    [InlineData(10, MovementType.Adjustment, "Stock check", 10)]
    public void AddMovement_WithValidInput_ShouldUpdateStockCorrectly(int quantity, MovementType type, string justification, int expectedStock)
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        product.AddMovement(quantity, type, justification);

        // Assert
        product.GetCurrentStock().Should().Be(expectedStock);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(100)]
    public void AddMovement_WhenOutboundExceedsCurrentStock_ShouldThrowDomainException(int outboundAmount)
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);
        product.AddMovement(10, MovementType.Inbound, "Initial stock");

        // Act
        Action act = () => product.AddMovement(outboundAmount, MovementType.Outbound, "Customer Order");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage(DomainErrors.Product.NegativeStockBalance);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddMovement_WhenAdjustingWithoutJustification_ShouldThrowDomainException(string? invalidJustification)
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        Action act = () => product.AddMovement(5, MovementType.Adjustment, invalidJustification);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage(DomainErrors.InventoryMovement.InvalidAdjustment);
    }
}
