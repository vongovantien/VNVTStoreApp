# ⚙️ VNVTStore Backend - Clean Architecture & DDD Core

[![Framework](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Architecture](https://img.shields.io/badge/Clean-Architecture-blue)](https://github.com/vongovantien/VNVTStoreApp)
[![Pattern](https://img.shields.io/badge/Pattern-CQRS-green)](https://github.com/vongovantien/VNVTStoreApp)
[![Database](https://img.shields.io/badge/PostgreSQL-PostgreSQL-336791?logo=postgresql)](https://www.postgresql.org/)

A professional, industrial-grade backend for the **VNVTStore** platform. Built with **.NET 8**, adhering to strict **Clean Architecture** and **Domain-Driven Design (DDD)** principles.

---

## 🏗️ Architectural Excellence

### Clean Architecture Layers
The project is strictly divided into four distinct layers to ensure separation of concerns and testability:
- **API**: ASP.NET Core Web API, Middleware, Controllers, and Swagger.
- **Infrastructure**: Entity Framework Core implementation, JWT Services, and Persistence logic.
- **Application**: MediatR commands/queries, Handlers, DTOs, and FluentValidation.
- **Domain**: Rich entities, business rules, interfaces, and value objects (**Zero Dependencies**).

### 🛡️ Domain-Driven Design (DDD)
We use **Rich Domain Models** instead of anemic models. All business logic is encapsulated within Entities.
- **Private Setters**: Ensures state can only be modified through valid domain methods.
- **Factory Methods**: Controlled instantiation via static `Create` methods using the **Result Pattern**.
- **Business Methods**: Entities like `TblCart` or `TblProduct` handle their own logic (`AddItem`, `DeductStock`).

### ⚡ Patterns & Optimization
- **CQRS**: Clean separation between read and write operations using **MediatR**.
- **Unit of Work & Repository**: Abstracted data access for persistence ignorance.
- **Sliding Refresh Tokens**: Secure authentication with automated token rotation.
- **Generic CRUD Base**: Standardized generic handlers for rapid entity management.

---

## 📁 Repository Structure

```text
VNVTStore.Backend/
├── src/
│   ├── VNVTStore.API/           # Entry point & Controllers
│   ├── VNVTStore.Application/   # Use cases & Handlers
│   ├── VNVTStore.Infrastructure/ # Database & external services
│   └── VNVTStore.Domain/        # Domain entities & business rules
└── tests/
    ├── VNVTStore.Application.Tests/ # Business logic validation
    └── VNVTStore.Domain.Tests/      # Entity-level unit tests
```

---

## 🚀 Development Setup

### 1. Database Configuration
Update the connection string in `VNVTStore.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=vnvtstore;Username=postgres;Password=admin"
}
```

### 2. Migrations & Database Init
```bash
cd VNVTStore.Backend
dotnet ef database update --project src/VNVTStore.Infrastructure --startup-project src/VNVTStore.API
```

### 3. Run the API
```bash
dotnet run --project src/VNVTStore.API/VNVTStore.API.csproj
```
> [!IMPORTANT]
> The API runs at `http://localhost:5178`. Access the Swagger UI at `/swagger`.

---

## 🧪 Testing Coverage
We employ a **Test-Driven** approach to ensure API stability.
```bash
# Run 130+ Business Logic Tests
dotnet test tests/VNVTStore.Application.Tests

# Run Core Domain Rule Tests
dotnet test tests/VNVTStore.Domain.Tests
```

---
<p align="center">Crafted for scalability by [vongovantien](https://github.com/vongovantien)</p>
