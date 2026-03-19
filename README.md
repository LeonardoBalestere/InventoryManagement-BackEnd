\# 📦 Inventory Management System - API



!\[.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)

!\[EF Core](https://img.shields.io/badge/Entity\_Framework-Core-purple.svg)

!\[Clean Architecture](https://img.shields.io/badge/Architecture-Clean-success.svg)



A RESTful API developed in C# and .NET 10 for inventory management. This project is part of a development portfolio and was built to demonstrate the application of software engineering best practices in back-end development.



The front-end of this application is being developed in Angular and will communicate exclusively with this API.



\## 🎯 Technical Objectives



This project goes beyond a traditional, tightly-coupled CRUD. The main focus is on code quality, maintainability, and scalability, applying:



\* \*\*Clean Architecture:\*\* Clear separation of concerns into layers (Domain, Application, Infrastructure, API).

\* \*\*SOLID Principles:\*\* Code focused on single responsibility, dependency injection, and inversion of control.

\* \*\*Design Patterns:\*\* Implementation of patterns such as the \*Repository Pattern\* and \*Unit of Work\*.

\* \*\*Security:\*\* Authentication/Authorization (JWT) implementation and route protection.

\* \*\*Global Exception Handling:\*\* Custom middleware for standardized error responses (Problem Details).



\## 🛠️ Technologies Used



\* \*\*Language:\*\* C# 12

\* \*\*Framework:\*\* .NET 10.0 (ASP.NET Core Web API)

\* \*\*ORM:\*\* Entity Framework Core

\* \*\*Database:\*\* SQL Server (or SQLite for initial local development)

\* \*\*API Documentation:\*\* Swagger / OpenAPI

\* \*\*Object Mapping:\*\* AutoMapper

\* \*\*Validation:\*\* FluentValidation



\## 📁 Solution Structure



The solution was divided following Clean Architecture guidelines:



\* `InventoryManagement.API`: Application entry point, Controllers, and global dependency injection.

\* `InventoryManagement.Application`: Use cases, DTOs, service interfaces, and validations.

\* `InventoryManagement.Domain`: The core of the application. Entities, repository interfaces, and pure business rules (no external dependencies).

\* `InventoryManagement.Infrastructure`: Data access implementations, Entity Framework, database mapping, and external integrations.



\## 🚀 How to Run Locally



\### Prerequisites

\* \[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

\* .NET CLI (or your preferred IDE)



\### Steps to run the API



1\. Clone this repository:

```bash

git clone \[https://github.com/YOUR-USERNAME/InventoryManagement.git](https://github.com/YOUR-USERNAME/InventoryManagement.git)

