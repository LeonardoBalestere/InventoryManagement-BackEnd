10. Centralized Configuration Management
Status: Accepted

Context: Storing Connection Strings, JWT secrets, or API keys in .env files or the repository violates compliance and security standards.

Decision: Centralized configuration and secrets management using Azure Key Vault accessed via Azure Managed Identities.

Consequences:
- Positive: We gain military-grade cryptographic storage and passwordless database authentication.
- Negative: We add a slight latency overhead during application startup to fetch secrets from the vault.
