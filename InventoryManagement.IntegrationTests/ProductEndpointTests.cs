using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Products.Commands.CreateProduct;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagement.IntegrationTests;

public class ProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IProductReadRepository> _mockProductReadRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public ProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockProductReadRepository = new Mock<IProductReadRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("Jwt:Key", "ThisIsASecretKeyForTestingPurposesOnly123!"),
                    new KeyValuePair<string, string?>("Jwt:Issuer", "TestIssuer"),
                    new KeyValuePair<string, string?>("Jwt:Audience", "TestAudience"),
                    new KeyValuePair<string, string?>("Auth:AdminUser", "admin"),
                    new KeyValuePair<string, string?>("Auth:AdminPass", "Admin123!"),
                    new KeyValuePair<string, string?>("Auth:ManagerUser", "manager"),
                    new KeyValuePair<string, string?>("Auth:ManagerPass", "Manager123!")
                });
            });
            builder.ConfigureServices(services =>
            {
                // Remove existing
                var repoDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IProductRepository));
                if (repoDesc != null) services.Remove(repoDesc);

                var readRepoDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IProductReadRepository));
                if (readRepoDesc != null) services.Remove(readRepoDesc);

                var uowDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IUnitOfWork));
                if (uowDesc != null) services.Remove(uowDesc);

                services.AddScoped<IProductRepository>(_ => _mockProductRepository.Object);
                services.AddScoped<IProductReadRepository>(_ => _mockProductReadRepository.Object);
                services.AddScoped<IUnitOfWork>(_ => _mockUnitOfWork.Object);
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("token").GetString() ?? string.Empty;
    }

    [Fact]
    public async Task GetProducts_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithAdminRole_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var command = new CreateProductCommand(Guid.NewGuid(), $"SKU-{Guid.NewGuid().ToString().Substring(0, 8)}", "Test Product", "Test Desc", 10.5m, 5);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithManagerRole_ReturnsForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync("manager", "Manager123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var command = new CreateProductCommand(Guid.NewGuid(), $"SKU-{Guid.NewGuid().ToString().Substring(0, 8)}", "Test Product", "Test Desc", 10.5m, 5);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
