using InventoryManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace InventoryManagement.Application.Common.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>, IIdempotentRequest
    where TResponse : notnull
{
    private readonly IDistributedCache _cache;

    public IdempotencyBehavior(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return await next();
        }

        var cacheKey = $"Idempotency:{request.IdempotencyKey}";
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedValue))
        {
            return JsonSerializer.Deserialize<TResponse>(cachedValue)!;
        }

        var response = await next();

        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(response), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, 
            cancellationToken);

        return response;
    }
}
