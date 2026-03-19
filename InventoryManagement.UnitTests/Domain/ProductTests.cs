using System;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using Xunit;

namespace InventoryManagement.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void AddMovement_OutboundCreatesNegativeStock_ThrowsDomainException()
    {
        // Arrange
        var product = new Product("SKU123", "Test Product", "Description", 10.0m, 5);
        product.AddMovement(10, MovementType.Inbound); // Establish 10 stock

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            product.AddMovement(15, MovementType.Outbound)
        );

        Assert.Equal("Outbound movement cannot result in a negative stock balance.", exception.Message);
    }

    [Fact]
    public void AddMovement_AdjustmentWithoutJustification_ThrowsDomainException()
    {
        // Arrange
        var product = new Product("SKU123", "Test Product", "Description", 10.0m, 5);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            product.AddMovement(5, MovementType.Adjustment, justification: "")
        );

        Assert.Equal("An adjustment movement requires a valid justification.", exception.Message);
    }

    [Fact]
    public void GetCurrentStock_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product("SKU123", "Test Product", "Description", 10.0m, 5);

        // Act
        product.AddMovement(20, MovementType.Inbound);
        product.AddMovement(5, MovementType.Outbound);
        product.AddMovement(-2, MovementType.Adjustment, "Stock correction");

        // Assert
        // 20 (in) - 5 (out) + (-2) (adj) = 13
        Assert.Equal(13, product.GetCurrentStock());
    }

    [Fact]
    public void Deactivate_PreventsNewMovements()
    {
        // Arrange
        var product = new Product("SKU123", "Test Product", "Description", 10.0m, 5);
        product.Deactivate();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            product.AddMovement(10, MovementType.Inbound)
        );

        Assert.Equal("Cannot process inventory movements for an inactive product.", exception.Message);
    }
}