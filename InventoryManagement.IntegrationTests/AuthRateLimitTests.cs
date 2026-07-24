using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagement.IntegrationTests;

[Trait("Category", "Integration")]
public class AuthRateLimitTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthRateLimitTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("RateLimiting:Auth:PermitLimit", "2"),
                    new KeyValuePair<string, string?>("RateLimiting:Auth:WindowSeconds", "300")
                });
            });
        });
    }

    [Fact]
    public async Task Login_ExceedingRateLimit_Returns429()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidLogin = new { Username = "nobody", Password = "wrong" };

        // Act: the first two attempts consume the window permits
        var first = await client.PostAsJsonAsync("/api/auth/login", invalidLogin);
        var second = await client.PostAsJsonAsync("/api/auth/login", invalidLogin);
        var third = await client.PostAsJsonAsync("/api/auth/login", invalidLogin);

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        third.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
