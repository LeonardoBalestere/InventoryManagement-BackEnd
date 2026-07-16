# 📦 Inventory Management System - API

![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)
![EF Core](https://img.shields.io/badge/Entity_Framework-Core-purple.svg)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-success.svg)

A RESTful API developed in C# and .NET 10 for inventory management. This project is part of a development portfolio and was built to demonstrate the application of software engineering best practices in back-end development.

The front-end of this application is being developed in Angular and will communicate exclusively with this API.

## 🎯 Technical Objectives

This project goes beyond a traditional, tightly-coupled CRUD. The main focus is on code quality, maintainability, and scalability, applying:

* **Clean Architecture:** Clear separation of concerns into layers (Domain, Application, Infrastructure, API).
* **SOLID Principles:** Code focused on single responsibility, dependency injection, and inversion of control.
* **Design Patterns:** *Repository Pattern*, *Unit of Work*, *CQRS* (MediatR), *Outbox Pattern*, and idempotent commands.
* **Security:** Authentication/Authorization (JWT with role-based policies), rate limiting, and input sanitization (Anti-XSS).
* **Resilience:** Polly retries and circuit breaker around the distributed cache (Redis).
* **Observability:** Structured logging (Serilog) and OpenTelemetry traces/metrics.
* **Global Exception Handling:** Custom middleware for standardized error responses (Problem Details).

## 🛠️ Technologies Used

* **Language:** C# 12 / C# 13
* **Framework:** .NET 10.0 (ASP.NET Core Web API)
* **ORM:** Entity Framework Core (automated migrations)
* **Database:** PostgreSQL
* **Cache:** Redis (`IDistributedCache`) with Polly resilience
* **Messaging:** Transactional Outbox + `BackgroundService` + SendGrid (low-stock email alerts)
* **API Documentation:** OpenAPI / Scalar
* **CQRS / Mediator:** MediatR
* **Validation:** FluentValidation
* **Logging & Telemetry:** Serilog (Seq / Elasticsearch) and OpenTelemetry
* **Tests:** xUnit, Moq, FluentAssertions, WebApplicationFactory

## 📁 Solution Structure

The solution was divided following Clean Architecture guidelines:

* `InventoryManagement.API`: Application entry point, Controllers, and global dependency injection.
* `InventoryManagement.Application`: Use cases, DTOs, service interfaces, and validations.
* `InventoryManagement.Domain`: The core of the application. Entities, repository interfaces, and pure business rules (no external dependencies).
* `InventoryManagement.Infrastructure`: Data access implementations, Entity Framework, database mapping, and external integrations.

## 🚀 How to Run Locally

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* .NET CLI (or your preferred IDE)

### Steps to run the API

1. Clone this repository:
```bash
git clone https://github.com/YOUR-USERNAME/InventoryManagement.git
```

2. Navigate to the backend folder:
```bash
cd InventoryManagement/backend
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Apply database migrations (if applicable):
```bash
dotnet ef database update --project InventoryManagement.Infrastructure --startup-project InventoryManagement.API
```

5. Run the application:
```bash
dotnet run --project InventoryManagement.API
```

The API will be up and running, and you can access the interactive Scalar documentation at `https://localhost:<port>/docs`.

## 📐 Architectural Decision Records

The key architecture and infrastructure decisions are documented in [docs/adr](docs/adr).

## 📈 Roadmap (Completed Scope)

- [x] Initial Domain modeling (Product, Category, Movement).
- [x] EF Core configuration and Migrations.
- [x] OpenTelemetry (OTel) implementation for observability.
- [x] Unit testing creation with xUnit and Moq.
- [x] Integration testing (WebApplicationFactory).
- [x] Application containerization with Docker.
- [x] JWT authentication with role-based authorization.
- [x] Idempotent commands backed by Redis (`IDistributedCache`).
- [x] Polly resilience (retry + circuit breaker) for the distributed cache (ADR-5).
- [x] Async low-stock email alerts via Outbox Pattern + SendGrid (ADR-9).

Remaining improvements are tracked as [GitHub issues](../../issues) (tech debt and enhancements backlog).

---
*Developed by alves*
