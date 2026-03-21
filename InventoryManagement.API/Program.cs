using InventoryManagement.API.Infrastructure;
using InventoryManagement.Application;
using InventoryManagement.Application.Products.Commands.AddInventoryMovement;
using InventoryManagement.Application.Products.Commands.CreateProduct;
using InventoryManagement.Application.Products.Queries.GetProducts;
using InventoryManagement.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["SeqUrl"] ?? "http://localhost:5341")
        .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticSearchUrl"] ?? "http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "inventorymanagement-logs-{0:yyyy.MM.dd}"
        });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options => 
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddRuntimeInstrumentation()
               .AddOtlpExporter();
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Inventory Management API";
        document.Info.Version = "v1";
        document.Info.Description = "Enterprise-level API for inventory control";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Bearer token."
        });

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
        });

        // Define the explicit order of tags in the UI
        document.Tags = new HashSet<OpenApiTag>
        {
            new() { Name = "Authentication", Description = "Endpoints for logging in and getting tokens" },
            new() { Name = "Products", Description = "Endpoints for managing products and stock movements" }
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("Inventory Management API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// ----- API ENDPOINTS -----
app.MapHealthChecks("/health");

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

var api = app.MapGroup("/api/v{version:apiVersion}/products")
    .WithApiVersionSet(apiVersionSet)
    .WithTags("Products")
    .RequireAuthorization();

// POST Login (Generates JWT Token for authentication)
app.MapPost("/api/auth/login", (LoginRequest request, Microsoft.Extensions.Configuration.IConfiguration config) =>
{
    var adminUser = config["Auth:AdminUser"] ?? throw new InvalidOperationException("Auth:AdminUser missing");
    var adminPass = config["Auth:AdminPass"] ?? throw new InvalidOperationException("Auth:AdminPass missing");
    var managerUser = config["Auth:ManagerUser"] ?? throw new InvalidOperationException("Auth:ManagerUser missing");
    var managerPass = config["Auth:ManagerPass"] ?? throw new InvalidOperationException("Auth:ManagerPass missing");

    if ((request.Username == adminUser && request.Password == adminPass) ||
        (request.Username == managerUser && request.Password == managerPass))
    {
        var role = request.Username == adminUser ? "Admin" : "Manager";

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, request.Username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
        };

        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return Results.Ok(new { Token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token) });
    }

    return Results.Unauthorized();
}).AllowAnonymous()
  .WithTags("Authentication");

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

if (app.Environment.EnvironmentName != "Testing")
{
    await app.ApplyMigrationsAndSeedAsync();
}
app.Run();

public record LoginRequest(string Username, string Password);
public record AddInventoryMovementRequest(string MovementType, int Quantity, string? Justification, string IdempotencyKey);
