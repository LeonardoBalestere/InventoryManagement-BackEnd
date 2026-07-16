using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Models;
using InventoryManagement.Infrastructure.BackgroundServices;
using InventoryManagement.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InventoryManagement.UnitTests.Infrastructure;

[Trait("Category", "Unit")]
public class OutboxDispatcherTests
{
    private readonly Mock<IOutboxMessageRepository> _repositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly OutboxDispatcher _dispatcher;

    public OutboxDispatcherTests()
    {
        _repositoryMock = new Mock<IOutboxMessageRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _dispatcher = new OutboxDispatcher(
            _repositoryMock.Object,
            _emailSenderMock.Object,
            Mock.Of<ILogger<OutboxDispatcher>>());
    }

    private static OutboxMessage CreateLowStockMessage(int attemptCount = 0)
    {
        var alert = new LowStockAlert(Guid.NewGuid(), "SKU123", "Test Product", 2, 5);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(LowStockAlert),
            Content = JsonSerializer.Serialize(alert),
            OccurredOnUtc = DateTime.UtcNow,
            AttemptCount = attemptCount
        };
    }

    [Fact]
    public async Task ProcessPendingAsync_LowStockMessage_SendsEmailAndMarksProcessed()
    {
        // Arrange
        var message = CreateLowStockMessage();
        _repositoryMock.Setup(repo => repo.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        // Act
        await _dispatcher.ProcessPendingAsync(CancellationToken.None);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().BeNull();
        message.AttemptCount.Should().Be(1);

        _emailSenderMock.Verify(sender => sender.SendAsync(
            It.Is<string>(subject => subject.Contains("Test Product") && subject.Contains("SKU123")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingAsync_EmailFails_RecordsErrorAndKeepsMessagePending()
    {
        // Arrange
        var message = CreateLowStockMessage();
        _repositoryMock.Setup(repo => repo.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(sender => sender.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SendGrid unavailable"));

        // Act
        await _dispatcher.ProcessPendingAsync(CancellationToken.None);

        // Assert
        message.ProcessedOnUtc.Should().BeNull();
        message.Error.Should().Be("SendGrid unavailable");
        message.AttemptCount.Should().Be(1);

        _repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingAsync_MaxAttemptsExceeded_DeadLettersMessage()
    {
        // Arrange: message already failed 4 times; the 5th failure dead-letters it
        var message = CreateLowStockMessage(attemptCount: 4);
        _repositoryMock.Setup(repo => repo.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(sender => sender.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SendGrid unavailable"));

        // Act
        await _dispatcher.ProcessPendingAsync(CancellationToken.None);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().Be("SendGrid unavailable");
        message.AttemptCount.Should().Be(5);
    }

    [Fact]
    public async Task ProcessPendingAsync_UnknownMessageType_RecordsError()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "SomethingElse",
            Content = "{}",
            OccurredOnUtc = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        // Act
        await _dispatcher.ProcessPendingAsync(CancellationToken.None);

        // Assert
        message.ProcessedOnUtc.Should().BeNull();
        message.Error.Should().Contain("SomethingElse");

        _emailSenderMock.Verify(sender => sender.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingAsync_NoPendingMessages_DoesNotSave()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage>());

        // Act
        await _dispatcher.ProcessPendingAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
