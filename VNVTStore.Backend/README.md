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

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### 2. Clone & Restore
```bash
git clone https://github.com/vongovantien/VNVTStoreApp.git
cd VNVTStoreApp/VNVTStore.Backend
dotnet restore
```

### 3. 🔐 Configure Secrets (CRITICAL)
**Development uses User Secrets** - secrets are stored outside the project to prevent accidental commits.

```bash
cd src/VNVTStore.API

# Set JWT Secret (minimum 32 characters)
dotnet user-secrets set "JwtSettings:SecretKey" "YourSuperSecretKey_MinimumLength32Chars_ChangeMe!"
dotnet user-secrets set "JwtSettings:Issuer" "VNVTStore"
dotnet user-secrets set "JwtSettings:Audience" "VNVTStoreUsers"

# Optional: Override database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=shoppingdb;Username=postgres;Password=YOUR_PASSWORD"
```

**Verify secrets:**
```bash
dotnet user-secrets list
```

📖 **For production deployment:** See [DEPLOYMENT_SECRETS.md](docs/DEPLOYMENT_SECRETS.md)

### 4. Database Setup
Update the connection string in `appsettings.json` (or use User Secrets):
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=shoppingdb;Username=postgres;Password=password"
}
```

**Apply migrations:**
```bash
cd src/VNVTStore.API
dotnet ef database update
```

### 5. Run the API
```bash
dotnet run --project src/VNVTStore.API
```
> [!IMPORTANT]
> The API runs at `http://localhost:5178`. Access Swagger UI at `/swagger` or Scalar at `/scalar/v1`.

---

## 🔒 Security Features

✅ **JWT Authentication** with sliding refresh tokens  
✅ **Role-Based Access Control (RBAC)** with dynamic permissions  
✅ **Rate Limiting** (100 req/min global, 5 req/10s for auth)  
✅ **XSS Protection** via HTML sanitization  
✅ **SQL Injection Prevention** with parameterized queries  
✅ **CORS** configured for specific origins  
✅ **Password Hashing** with BCrypt  
✅ **User Secrets** for development, Key Vault for production

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
