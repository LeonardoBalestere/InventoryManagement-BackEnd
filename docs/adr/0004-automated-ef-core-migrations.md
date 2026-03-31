4. Automated EF Core Migrations
Status: Accepted

Context: Immediate coupling to the migration system during prototyping causes Model Drift, but using EnsureCreated in production environments poses a critical architectural risk of data loss and schema mismatch.

Decision: Automated EF Core Migrations for database schema evolution, abandoning EnsureCreatedAsync.

Consequences:
- Positive: We gain deterministic, idempotent schema deployments and a strict CI/CD alignment for DevOps data management.
- Negative: We lose the extreme speed of dynamic database recreation during the initial domain engineering phase.
