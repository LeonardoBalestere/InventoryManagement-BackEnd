using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using System;
using Xunit;

namespace InventoryManagement.Domain.UnitTests.Entities;

public class InventoryMovementTests
{
    private readonly Guid _validProductId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidInbound_ShouldCreateMovement()
    {
        // Act
        var movement = new InventoryMovement(_validProductId, MovementType.Inbound, 10, null);

        // Assert
        movement.Should().NotBeNull();
        movement.Id.Should().NotBe(Guid.Empty);
        movement.ProductId.Should().Be(_validProductId);
        movement.Type.Should().Be(MovementType.Inbound);
        movement.Quantity.Should().Be(10);
        movement.MovementDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(MovementType.Inbound, 0)]
    [InlineData(MovementType.Inbound, -5)]
    [InlineData(MovementType.Outbound, 0)]
    [InlineData(MovementType.Outbound, -10)]
    public void Constructor_WithInvalidInboundOrOutboundQuantity_ShouldThrowDomainException(MovementType type, int quantity)
    {
        // Act
        Action act = () => new InventoryMovement(_validProductId, type, quantity, null);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage($"{type} quantity must be greater than zero.");
    }

    [Fact]
    public void Constructor_AdjustmentWithZeroQuantity_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new InventoryMovement(_validProductId, MovementType.Adjustment, 0, "Valid justification");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Adjustment quantity cannot be zero.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_AdjustmentWithoutJustification_ShouldThrowDomainException(string? invalidJustification)
    {
        // Act
        Action act = () => new InventoryMovement(_validProductId, MovementType.Adjustment, 10, invalidJustification);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("An adjustment movement requires a valid justification.");
    }

    [Fact]
    public void Constructor_EmptyProductId_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new InventoryMovement(Guid.Empty, MovementType.Inbound, 10, null);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("ProductId cannot be empty.");
    }
}
