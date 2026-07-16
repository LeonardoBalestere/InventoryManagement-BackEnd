using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using InventoryManagement.Application.Products.Commands.AddInventoryMovement;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Moq;
using Xunit;

namespace InventoryManagement.UnitTests.Application;

[Trait("Category", "Unit")]
public class AddInventoryMovementCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILowStockAlertOutbox> _lowStockAlertOutboxMock;
    private readonly AddInventoryMovementCommandHandler _handler;

    public AddInventoryMovementCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _lowStockAlertOutboxMock = new Mock<ILowStockAlertOutbox>();
        _handler = new AddInventoryMovementCommandHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _lowStockAlertOutboxMock.Object);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new AddInventoryMovementCommand(Guid.NewGuid(), "Inbound", 10, null, "1234");
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("Inbound", 10, 10, "Inbound test")]
    [InlineData("Adjustment", 5, 5, "Stock re-eval")]
    public async Task Handle_ValidCommand_AddsMovementAndSaves(string movementType, int quantity, int expectedStock, string justification)
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "SKU123", "Test Product", "Description", 10.0m, 5);
        var command = new AddInventoryMovementCommand(product.Id, movementType, quantity, justification, Guid.NewGuid().ToString());

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(product.Id);
        product.GetCurrentStock().Should().Be(expectedStock);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockCrossesBelowMinLevel_EnqueuesLowStockAlert()
    {
        // Arrange: stock 10, min level 5; outbound 8 leaves 2 (crosses below)
        var product = new Product(Guid.NewGuid(), "SKU123", "Test Product", "Description", 10.0m, 5);
        product.AddMovement(10, MovementType.Inbound, "Initial stock");

        var command = new AddInventoryMovementCommand(product.Id, "Outbound", 8, "Customer order", Guid.NewGuid().ToString());

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lowStockAlertOutboxMock.Verify(outbox => outbox.EnqueueAsync(
            It.Is<LowStockAlert>(alert =>
                alert.ProductId == product.Id &&
                alert.Sku == product.Sku &&
                alert.CurrentStock == 2 &&
                alert.MinStockLevel == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockStaysAboveMinLevel_DoesNotEnqueueAlert()
    {
        // Arrange: stock 10, min level 5; outbound 2 leaves 8 (still above)
        var product = new Product(Guid.NewGuid(), "SKU123", "Test Product", "Description", 10.0m, 5);
        product.AddMovement(10, MovementType.Inbound, "Initial stock");

        var command = new AddInventoryMovementCommand(product.Id, "Outbound", 2, "Customer order", Guid.NewGuid().ToString());

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lowStockAlertOutboxMock.Verify(outbox => outbox.EnqueueAsync(
            It.IsAny<LowStockAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StockAlreadyBelowMinLevel_DoesNotEnqueueAlertAgain()
    {
        // Arrange: stock 0 (already below min 5); inbound 1 leaves 1 (still below, no new crossing)
        var product = new Product(Guid.NewGuid(), "SKU123", "Test Product", "Description", 10.0m, 5);

        var command = new AddInventoryMovementCommand(product.Id, "Inbound", 1, "Partial restock", Guid.NewGuid().ToString());

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lowStockAlertOutboxMock.Verify(outbox => outbox.EnqueueAsync(
            It.IsAny<LowStockAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
