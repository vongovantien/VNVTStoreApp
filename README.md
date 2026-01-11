# VNVTStore - Modern E-Commerce Platform

VNVTStore is a full-stack e-commerce solution built with modern technologies, following **Domain-Driven Design (DDD)** and **Clean Architecture** principles on the backend, and a robust component-driven approach on the frontend.

## 🏗️ Project Architecture

This repository uses a monorepo-style structure:
- **[VNVTStore.Backend](VNVTStore.Backend)**: .NET 8 Web API.
    - **Architecture**: Clean Architecture + CQRS + Rich Domain Models.
    - **Key Principles**: 
        - **Rich Domain Models**: Entities (`TblCart`, `TblOrder`, `TblProduct`) encapsulate business logic and use **Private Setters** to ensure integrity.
        - **Factory Methods**: Controlled object creation via static `Create` methods using `Result` pattern.
        - **Unit of Work & Repository**: Abstractions for data access.
- **[VNVTStore.Frontend](VNVTStore.Frontend)**: React 19 + Vite 7 + TypeScript.
    - **Architecture**: Feature-Sliced Design inspiration.
    - **Key Features**: 
        - **Axios Service**: Centralized API client with **Interceptors**, **Auto-Refresh Token** handling (Retry Queue), and standardized Error handling.
        - **State Management**: Zustand.

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### 1. Backend Setup (`/VNVTStore.Backend`)
```bash
cd VNVTStore.Backend
# Restore dependencies
dotnet restore

# Build the API
dotnet build src/VNVTStore.API/VNVTStore.API.csproj

# Run Tests (Strict Validation)
dotnet test src/VNVTStore.Tests/VNVTStore.Tests.csproj

# Run the API
dotnet run --project src/VNVTStore.API/VNVTStore.API.csproj
```
API Documentation (Swagger) will be available at `http://localhost:5178/swagger`.

### 2. Frontend Setup (`/VNVTStore.Frontend`)
```bash
cd VNVTStore.Frontend
# Install dependencies
npm install

# Run Tests (Vitest)
npm test

# Start Development Server
npm run dev
```
Frontend will be available at `http://localhost:5173`.

## 🔄 CI/CD Pipeline (GitHub Actions)

This project includes a fully configured CI/CD pipeline in [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml).
It triggers on `push` and `pull_request` to `main/master`.

### Automation Steps:
1.  **Backend Pipeline**:
    -   Restores NuGet packages.
    -   Builds the Solution.
    -   Runs Unit Tests (Clean Architecture Compliance).
2.  **Frontend Pipeline**:
    -   Installs NPM packages.
    -   Runs `vitest` suite.
    -   Builds the Production bundle (`vite build`).

## 📜 Features

- **Authentication**: Secure JWT with **Auto-Refresh** mechanism on the frontend.
- **Rich Domain Logic**: Stock management (`DeductStock`, `RestoreStock`), Cart logic (`AddItem`, `Clear`) encapsulated in Entities.
- **Checkout Process**: Multi-step checkout with address management and shipping fee calculation.
- **Admin Panel**: Manage products, categories, orders, customers, and system statistics.

## 🛠️ Technology Stack

| Part | Technologies |
|------|--------------|
| **Backend** | .NET 8, EF Core 8, PostgreSQL, MediatR, AutoMapper, FluentValidation, Asp.Versioning, xUnit, Moq |
| **Frontend** | React 19, Vite 7, TypeScript, Axios (with Interceptors), Tailwind CSS 4, Zustand, React Query, React Hook Form, Zod |
| **Patterns** | Clean Architecture, CQRS, Repository, Unit of Work, Domain-Driven Design (Rich Models) |
| **DevOps** | GitHub Actions (CI/CD), Docker Support |

---
Managed by [vongovantien](https://github.com/vongovantien).
