# VNVTStore - E-Commerce Platform

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-purple" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Architecture-Clean%20Architecture-blue" alt="Clean Architecture" />
  <img src="https://img.shields.io/badge/Pattern-CQRS-green" alt="CQRS" />
  <img src="https://img.shields.io/badge/Database-PostgreSQL-336791" alt="PostgreSQL" />
</p>

## 📋 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Kiến trúc](#kiến-trúc)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Cài đặt](#cài-đặt)
- [API Documentation](#api-documentation)
- [Contributing](#contributing)

## 🚀 Giới thiệu

VNVTStore là nền tảng thương mại điện tử được xây dựng với **.NET 8** theo **Clean Architecture** và **CQRS pattern**. Dự án bao gồm:

- 🛒 **Phân hệ người dùng**: Xem sản phẩm, giỏ hàng, đặt hàng, thanh toán
- 👨‍💼 **Phân hệ quản trị**: Quản lý sản phẩm, đơn hàng, người dùng, báo cáo

## 🛠️ Công nghệ sử dụng

### Backend
| Công nghệ | Version | Mô tả |
|-----------|---------|-------|
| .NET | 8.0 | Framework chính |
| Entity Framework Core | 8.0.7 | ORM |
| PostgreSQL | Latest | Database |
| MediatR | 12.4.0 | CQRS/Mediator pattern |
| FluentValidation | 11.9.2 | Validation |
| AutoMapper | 13.0.1 | Object mapping |
| JWT Bearer | 8.0.7 | Authentication |
| Swagger | 6.6.2 | API Documentation |

### Patterns & Principles
- ✅ Clean Architecture
- ✅ CQRS (Command Query Responsibility Segregation)
- ✅ Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Result Pattern (Railway Oriented Programming)
- ✅ Domain-Driven Design concepts

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  (Controllers, Middlewares, Extensions)                      │
├─────────────────────────────────────────────────────────────┤
│                     Application Layer                        │
│  (Commands, Queries, Handlers, DTOs, Validators)            │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│  (Entities, Value Objects, Enums, Interfaces)               │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  (DbContext, Repositories, Services, Configurations)        │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Flow

```
API → Application → Domain ← Infrastructure
```

- **Domain Layer**: Không phụ thuộc vào layer nào khác
- **Application Layer**: Chỉ phụ thuộc vào Domain
- **Infrastructure Layer**: Phụ thuộc vào Domain và Application
- **API Layer**: Phụ thuộc vào Application và Infrastructure

## 📁 Cấu trúc dự án

```
VNVTStore/
├── VNVTStore.sln
└── src/
    ├── VNVTStore.Domain/
    │   ├── Common/           # BaseEntity, AggregateRoot, ValueObject
    │   ├── Entities/         # User, Product, Order, Cart, etc.
    │   ├── Enums/            # OrderStatus, PaymentStatus, UserRole
    │   ├── ValueObjects/     # Money, Address
    │   └── Interfaces/       # IRepository, IUnitOfWork
    │
    ├── VNVTStore.Application/
    │   ├── Common/           # Result, PagedResult, Behaviors
    │   ├── Auth/             # Commands, Queries, DTOs, Validators
    │   ├── Products/         # Commands, Queries, DTOs, Validators
    │   ├── Orders/           # Commands, Queries, DTOs
    │   ├── Cart/             # Commands, Queries, DTOs
    │   ├── Categories/       # Commands, Queries, DTOs
    │   ├── Reviews/          # Commands, Queries, DTOs
    │   ├── Users/            # Commands, Queries, DTOs
    │   └── Mappings/         # AutoMapper Profiles
    │
    ├── VNVTStore.Infrastructure/
    │   ├── Persistence/
    │   │   ├── Configurations/  # Entity Configurations (Fluent API)
    │   │   ├── Repositories/    # Repository Implementations
    │   │   ├── ApplicationDbContext.cs
    │   │   └── UnitOfWork.cs
    │   └── Services/         # JwtService, PasswordHasher
    │
    └── VNVTStore.API/
        ├── Controllers/v1/   # API Controllers với versioning
        ├── Middlewares/      # Exception Handling
        └── Extensions/       # Service Collection Extensions
```

## 💻 Cài đặt

### Yêu cầu
- .NET 8.0 SDK
- PostgreSQL 14+
- Docker (optional)

### Bước 1: Clone repository

```bash
git clone https://github.com/vongovantien/VNVTStoreApp.git
cd VNVTStoreApp/VNVTStore
```

### Bước 2: Cấu hình database

Cập nhật connection string trong `src/VNVTStore.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=vnvtstore;Username=postgres;Password=your_password"
  }
}
```

### Bước 3: Chạy migrations

```bash
cd src/VNVTStore.API
dotnet ef migrations add InitialCreate --project ../VNVTStore.Infrastructure
dotnet ef database update --project ../VNVTStore.Infrastructure
```

### Bước 4: Chạy ứng dụng

```bash
dotnet run --project src/VNVTStore.API
```

Ứng dụng sẽ chạy tại `https://localhost:5001` với Swagger UI.

## 📚 API Documentation

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Đăng ký tài khoản |
| POST | `/api/v1/auth/login` | Đăng nhập |

### Products

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/products` | Lấy danh sách sản phẩm | ❌ |
| GET | `/api/v1/products/{id}` | Lấy chi tiết sản phẩm | ❌ |
| POST | `/api/v1/products` | Tạo sản phẩm mới | Admin |
| PUT | `/api/v1/products/{id}` | Cập nhật sản phẩm | Admin |
| DELETE | `/api/v1/products/{id}` | Xóa sản phẩm | Admin |

### Categories

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/categories` | Lấy danh sách danh mục | ❌ |
| POST | `/api/v1/categories` | Tạo danh mục mới | Admin |

### Orders

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/orders` | Lấy danh sách đơn hàng | ✅ |
| POST | `/api/v1/orders` | Tạo đơn hàng mới | ✅ |
| PUT | `/api/v1/orders/{id}/status` | Cập nhật trạng thái | Admin |

### Quotes

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/quotes` | Lấy danh sách yêu cầu báo giá của tôi | ✅ |
| POST | `/api/v1/quotes` | Tạo yêu cầu báo giá mới | ✅ |

### Cart

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/cart` | Lấy giỏ hàng | ✅ |
| POST | `/api/v1/cart/items` | Thêm vào giỏ hàng | ✅ |
| PUT | `/api/v1/cart/items/{productId}` | Cập nhật số lượng | ✅ |
| DELETE | `/api/v1/cart/items/{productId}` | Xóa khỏi giỏ hàng | ✅ |

## 🔐 Authentication

API sử dụng JWT Bearer Token. Thêm header sau khi đăng nhập:

```
Authorization: Bearer <your_token>
```

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Tạo Pull Request

## 📄 License

This project is licensed under the MIT License.

## 📧 Contact

- Email: vongovantien@gmail.com
- GitHub: [@vongovantien](https://github.com/vongovantien)
