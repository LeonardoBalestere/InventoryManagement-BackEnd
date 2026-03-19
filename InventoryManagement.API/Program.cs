using InventoryManagement.Application;
using InventoryManagement.Infrastructure;
using InventoryManagement.API.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System;
using System.Text;
using MediatR;
using InventoryManagement.Application.Products.Commands.CreateProduct;
using InventoryManagement.Application.Products.Commands.AddInventoryMovement;
using InventoryManagement.Application.Products.Queries.GetProducts;

var builder = WebApplication.CreateBuilder(args);

// 1. Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Add Exception Handling (RFC 7807 problem details)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddRuntimeInstrumentation();
    });

// 4. Add Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "InventoryAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "InventoryClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "supersecretkey_inventoryapi12345!"))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Maps to the registered IExceptionHandler

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ----- API ENDPOINTS -----
var api = app.MapGroup("/api/products").RequireAuthorization();

// GET Products (Pagination)
api.MapGet("/", async ([FromQuery] string? searchTerm, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, IMediator mediator) =>
{
    var query = new GetProductsQuery(searchTerm, pageNumber ?? 1, pageSize ?? 10);
    return Results.Ok(await mediator.Send(query));
});

// POST Product (Admin only)
api.MapPost("/", async ([FromBody] CreateProductCommand command, IMediator mediator) =>
{
    var productId = await mediator.Send(command);
    return Results.Created($"/api/products/{productId}", new { Id = productId });
}).RequireAuthorization("AdminOnly");

// POST Movement (Manager or Admin)
api.MapPost("/{id}/movements", async (Guid id, [FromBody] AddInventoryMovementRequest request, IMediator mediator) =>
{
    var command = new AddInventoryMovementCommand(id, request.MovementType, request.Quantity, request.Justification, request.IdempotencyKey);
    var result = await mediator.Send(command);
    return Results.Ok(new { ProductId = result });
}).RequireAuthorization("ManagerOrAdmin");

app.Run();

// DTO representing the inbound movement request, keeping ProductId out of the body
public record AddInventoryMovementRequest(string MovementType, int Quantity, string? Justification, string IdempotencyKey);
