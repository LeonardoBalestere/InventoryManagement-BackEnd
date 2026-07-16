using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InventoryManagement.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Polly;
using Polly.Retry;
using Xunit;

namespace InventoryManagement.UnitTests.Infrastructure;

[Trait("Category", "Unit")]
public class ResilientDistributedCacheTests
{
    private readonly Mock<IDistributedCache> _innerCacheMock = new();

    private ResilientDistributedCache CreateSut()
    {
        // Same shape as the production pipeline, but without delays to keep tests fast
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero
            })
            .Build();

        return new ResilientDistributedCache(_innerCacheMock.Object, pipeline);
    }

    [Fact]
    public async Task GetAsync_TransientFailures_RetriesAndReturnsValue()
    {
        // Arrange: fails twice, then succeeds
        var expected = Encoding.UTF8.GetBytes("cached-value");
        var attempts = 0;

        _innerCacheMock.Setup(cache => cache.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                attempts++;
                if (attempts < 3)
                    throw new TimeoutException("Transient network failure");
                return expected;
            });

        var sut = CreateSut();

        // Act
        var result = await sut.GetAsync("key");

        // Assert
        result.Should().BeEquivalentTo(expected);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task GetAsync_PersistentFailure_ThrowsAfterExhaustingRetries()
    {
        // Arrange
        _innerCacheMock.Setup(cache => cache.GetAsync("key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Redis is down"));

        var sut = CreateSut();

        // Act
        Func<Task> act = async () => await sut.GetAsync("key");

        // Assert: 1 original attempt + 3 retries
        await act.Should().ThrowAsync<TimeoutException>();
        _innerCacheMock.Verify(cache => cache.GetAsync("key", It.IsAny<CancellationToken>()), Times.Exactly(4));
    }

    [Fact]
    public async Task SetAsync_DelegatesToInnerCache()
    {
        // Arrange
        var value = Encoding.UTF8.GetBytes("value");
        var options = new DistributedCacheEntryOptions();
        var sut = CreateSut();

        // Act
        await sut.SetAsync("key", value, options);

        // Assert
        _innerCacheMock.Verify(cache => cache.SetAsync("key", value, options, It.IsAny<CancellationToken>()), Times.Once);
    }
}
