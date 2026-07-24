using FluentAssertions;
using InventoryManagement.API.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagement.IntegrationTests;

[Trait("Category", "Unit")]
public class GlobalExceptionHandlerTests
{
    private static async Task<(int StatusCode, string Body)> HandleAsync(Exception exception)
    {
        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_DoesNotLeakExceptionDetails()
    {
        var (statusCode, body) = await HandleAsync(new InvalidOperationException("sensitive-internal-detail"));

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.Should().NotContain("sensitive-internal-detail");
        body.Should().NotContain("StackTrace").And.NotContain(" at ");
        body.Should().Contain("traceId");
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ReturnsGenericDetailWithTraceId()
    {
        var (statusCode, body) = await HandleAsync(new Exception("boom"));

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.Should().Contain("An unexpected error occurred.");
        body.Should().Contain("traceId");
        body.Should().NotContain("boom");
    }
}
