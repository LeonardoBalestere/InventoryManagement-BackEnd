using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using System;
using Xunit;

namespace InventoryManagement.Domain.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateProduct()
    {
        // Arrange & Act
        var product = new Product("SKU123", "Laptop", "A fast laptop", 1500m, 10);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.Sku.Should().Be("SKU123");
        product.Name.Should().Be("Laptop");
        product.Description.Should().Be("A fast laptop");
        product.BasePrice.Should().Be(1500m);
        product.MinStockLevel.Should().Be(10);
        product.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("123456789012345678901")] // >20
    public void Constructor_WithInvalidSku_ShouldThrowDomainException(string? invalidSku)
    {
        // Act
        Action act = () => new Product(invalidSku, "Name", "Desc", 100m, 0);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("SKU is required and cannot exceed 20 characters.");
    }

    [Fact]
    public void AddMovement_WithOutboundGreaterThanCurrentStock_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("SKU", "Name", "Desc", 100m, 0);
        product.AddMovement(10, MovementType.Inbound);

        // Act
        Action act = () => product.AddMovement(11, MovementType.Outbound);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Outbound movement cannot result in a negative stock balance.");
    }

    [Fact]
    public void AddMovement_ValidInbound_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product("SKU", "Name", "Desc", 100m, 0);

        // Act
        product.AddMovement(5, MovementType.Inbound);

        // Assert
        product.GetCurrentStock().Should().Be(5);
        product.Movements.Should().HaveCount(1);
    }
}
