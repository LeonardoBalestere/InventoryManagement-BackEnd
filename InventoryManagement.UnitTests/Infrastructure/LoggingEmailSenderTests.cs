using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InventoryManagement.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InventoryManagement.UnitTests.Infrastructure;

[Trait("Category", "Unit")]
public class LoggingEmailSenderTests
{
    [Fact]
    public async Task SendAsync_LogsSubjectInsteadOfSending()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingEmailSender>>();
        var sender = new LoggingEmailSender(loggerMock.Object);

        // Act
        var act = async () => await sender.SendAsync("Low stock alert", "<p>body</p>", CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Low stock alert")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
