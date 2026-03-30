using System;
using System.Collections.Generic;
using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using Xunit;

namespace InventoryManagement.UnitTests.Domain;

[Trait("Category", "Unit")]
public class ProductTests
{
    private static Product CreateValidProduct() =>
        new Product(Guid.NewGuid(), "SKU123", "Test Product", "Description", 10.0m, 5);

    [Theory]
    [InlineData(10, 11)]
    [InlineData(0, 1)]
    [InlineData(5, 100)]
    public void AddMovement_OutboundCreatesNegativeStock_ThrowsDomainException(int initialStock, int outboundQuantity)
    {
        // Arrange
        var product = CreateValidProduct();
        if (initialStock > 0)
        {
            product.AddMovement(initialStock, MovementType.Inbound);
        }

        // Act
        Action act = () => product.AddMovement(outboundQuantity, MovementType.Outbound);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Outbound movement cannot result in a negative stock balance.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void AddMovement_AdjustmentWithoutJustification_ThrowsDomainException(string invalidJustification)
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        Action act = () => product.AddMovement(5, MovementType.Adjustment, justification: invalidJustification);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("An adjustment movement requires a valid justification.");
    }

    public static IEnumerable<object[]> StockCalculationScenarios()
    {
        // Format: InboundQuantity, OutboundQuantity, AdjustmentQuantity, ExpectedFinalStock
        yield return new object[] { 20, 5, -2, 13 };
        yield return new object[] { 100, 50, 10, 60 };
        yield return new object[] { 10, 10, 0, 0 };
        yield return new object[] { 50, 0, -50, 0 };
    }

    [Theory]
    [MemberData(nameof(StockCalculationScenarios))]
    public void GetCurrentStock_CalculatesCorrectly(int inbound, int outbound, int adjustment, int expectedTotal)
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        if (inbound > 0) product.AddMovement(inbound, MovementType.Inbound);
        if (outbound > 0) product.AddMovement(outbound, MovementType.Outbound);
        if (adjustment != 0) product.AddMovement(adjustment, MovementType.Adjustment, "Stock correction");

        // Assert
        product.GetCurrentStock().Should().Be(expectedTotal);
    }

    [Theory]
    [InlineData(MovementType.Inbound, 10, null)]
    [InlineData(MovementType.Outbound, 5, null)]
    [InlineData(MovementType.Adjustment, 2, "Correction")]
    [InlineData(MovementType.Adjustment, -2, "Correction")]
    public void Deactivate_PreventsNewMovements(MovementType movementType, int quantity, string justification)
    {
        // Arrange
        var product = CreateValidProduct();

        // Establish sufficient stock first if testing outbound
        if (movementType == MovementType.Outbound)
        {
            product.AddMovement(quantity, MovementType.Inbound);
        }

        product.Deactivate();

        // Act
        Action act = () => product.AddMovement(quantity, movementType, justification);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot process inventory movements for an inactive product.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ThrowsDomainException(string invalidName)
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        Action act = () => new Product(categoryId, "SKU123", invalidName!, "Description", 10.0m, 5);

        // Assert
        act.Should().Throw<DomainException>(); 
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-100.5)]
    [InlineData(-0.01)]
    public void Constructor_WithNegativePrice_ThrowsDomainException(decimal invalidPrice)
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        Action act = () => new Product(categoryId, "SKU123", "Valid Name", "Description", invalidPrice, 5);

        // Assert
        act.Should().Throw<DomainException>();
    }
}