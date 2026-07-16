using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace InventoryManagement.Infrastructure.Caching;

/// <summary>
/// Decorates <see cref="IDistributedCache"/> with Polly retries and a circuit breaker (ADR-5),
/// shielding the application from transient network faults between the API and Redis.
/// </summary>
public sealed class ResilientDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _inner;
    private readonly ResiliencePipeline _pipeline;

    public ResilientDistributedCache(IDistributedCache inner, ILogger<ResilientDistributedCache> logger)
        : this(inner, BuildDefaultPipeline(logger))
    {
    }

    internal ResilientDistributedCache(IDistributedCache inner, ResiliencePipeline pipeline)
    {
        _inner = inner;
        _pipeline = pipeline;
    }

    private static ResiliencePipeline BuildDefaultPipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "Transient cache failure. Retry attempt {Attempt}.", args.AttemptNumber + 1);
                    return default;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(15),
                OnOpened = args =>
                {
                    logger.LogError(args.Outcome.Exception,
                        "Cache circuit opened for {BreakDuration}s.", args.BreakDuration.TotalSeconds);
                    return default;
                }
            })
            .Build();
    }

    public byte[]? Get(string key) =>
        _pipeline.Execute(() => _inner.Get(key));

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        await _pipeline.ExecuteAsync(async ct => await _inner.GetAsync(key, ct), token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        _pipeline.Execute(() => _inner.Set(key, value, options));

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        await _pipeline.ExecuteAsync(async ct => await _inner.SetAsync(key, value, options, ct), token);

    public void Refresh(string key) =>
        _pipeline.Execute(() => _inner.Refresh(key));

    public async Task RefreshAsync(string key, CancellationToken token = default) =>
        await _pipeline.ExecuteAsync(async ct => await _inner.RefreshAsync(key, ct), token);

    public void Remove(string key) =>
        _pipeline.Execute(() => _inner.Remove(key));

    public async Task RemoveAsync(string key, CancellationToken token = default) =>
        await _pipeline.ExecuteAsync(async ct => await _inner.RemoveAsync(key, ct), token);
}
