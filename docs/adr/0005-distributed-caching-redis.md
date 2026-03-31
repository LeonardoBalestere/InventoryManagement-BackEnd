5. Distributed Caching with Redis and Polly
Status: Accepted

Context: Unacceptable risk of inventory corruption due to duplicated POST requests. Need to relieve the PostgreSQL database from repetitive catalog read queries while handling potential network instability between the Azure VM and the Redis Cloud instance.

Decision: Distributed caching via Redis Enterprise Cloud (GitHub Student Pack) using IDistributedCache, combined with Polly for transient fault handling (Circuit Breaker/Retries).

Consequences:
- Positive: We gain strict state consistency, sub-millisecond read latency for catalogs, and high resilience against third-party API timeouts.
- Negative: We add structural complexity to the request flow and dependency on an external cloud cache provider.
