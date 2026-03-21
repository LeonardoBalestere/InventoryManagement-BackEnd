using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using Xunit;

namespace InventoryManagement.IntegrationTests;

public class DomainRulesTests
{
    private readonly Guid _validCategoryId = Guid.NewGuid();
    private readonly string _validSku = "PROD-123";
    private readonly string _validName = "Test Product";
    private readonly string _validDesc = "A test product";
    private readonly decimal _validPrice = 10.50m;
    private readonly int _validMinStock = 5;

    [Fact]
    public void Product_AddMovement_Outbound_ShouldThrowException_WhenResultingStockIsNegative()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        var exception = Assert.Throws<DomainException>(() => product.AddMovement(10, MovementType.Outbound));

        // Assert
        Assert.Equal("Outbound movement cannot result in a negative stock balance.", exception.Message);
    }

    [Fact]
    public void Product_AddMovement_Inbound_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        product.AddMovement(10, MovementType.Inbound);

        // Assert
        Assert.Equal(10, product.GetCurrentStock());
    }

    [Fact]
    public void InventoryMovement_Adjustment_ShouldThrowException_WhenJustificationIsEmpty()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        var exception = Assert.Throws<DomainException>(() => product.AddMovement(5, MovementType.Adjustment, ""));

        // Assert
        Assert.Equal("An adjustment movement requires a valid justification.", exception.Message);
    }

    [Fact]
    public void Product_Deactivate_ShouldPreventMovements()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);
        product.Deactivate();

        // Act
        var exception = Assert.Throws<DomainException>(() => product.AddMovement(5, MovementType.Inbound));

        // Assert
        Assert.Equal("Cannot process inventory movements for an inactive product.", exception.Message);
    }

    [Fact]
    public void Product_Constructor_ShouldThrowException_WhenSkuIsTooLong()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() => new Product(_validCategoryId, new string('A', 21), _validName, _validDesc, _validPrice, _validMinStock));

        // Assert
        Assert.Equal("SKU is required and cannot exceed 20 characters.", exception.Message);
    }

    [Fact]
    public void Product_Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() => new Product(_validCategoryId, _validSku, "", _validDesc, _validPrice, _validMinStock));

        // Assert
        Assert.Equal("Name is required and cannot exceed 100 characters.", exception.Message);
    }

    [Fact]
    public void Product_ChangePrice_ShouldUpdatePrice()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        product.ChangePrice(20.00m);

        // Assert
        Assert.Equal(20.00m, product.BasePrice);
    }

    [Fact]
    public void Product_ChangePrice_ShouldThrowException_WhenPriceIsNegative()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        var exception = Assert.Throws<DomainException>(() => product.ChangePrice(-5.00m));

        // Assert
        Assert.Equal("Base price cannot be negative.", exception.Message);
    }

    [Fact]
    public void Product_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = new Product(_validCategoryId, _validSku, _validName, _validDesc, _validPrice, _validMinStock);

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
    }
}
