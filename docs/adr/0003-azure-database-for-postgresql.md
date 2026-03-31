3. Azure Database for PostgreSQL
Status: Accepted

Context: Need for strong transactional consistency (ACID) for physical inventory movements, combined with strict Zero Trust network security. The database must not be exposed to the public internet to prevent brute-force attacks.

Decision: Azure Database for PostgreSQL Flexible Server (B1ms Tier) managed by Entity Framework Core 10, deployed within a delegated Azure Virtual Network (VNet) with Private Access.

Consequences:
- Positive: We gain relational integrity, optimized queries delegated to the ORM, and absolute network-level security.
- Negative: We lose the ease of connecting to the database directly from local developer machines without a VPN or SSH tunnel.
