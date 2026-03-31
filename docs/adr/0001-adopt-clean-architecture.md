1. Adopt Clean Architecture
Status: Accepted

Context: Need to isolate the business domain (inventory management) from delivery technologies (APIs, databases) and frameworks, ensuring long-term maintainability, technological independence, and high testability (xUnit).

Decision: Clean Architecture structured in 4 layers (API, Application, Domain, Infrastructure) and SOLID principles in .NET 10.

Consequences:
- Positive: We gain high testability, low coupling, and flexibility to replace infrastructure providers.
- Negative: We lose initial development speed due to architectural verbosity and proliferation of abstractions (interfaces, mappings) in simple use cases (basic CRUDs).
