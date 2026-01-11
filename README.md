# VNVTStore - Modern E-Commerce Platform

VNVTStore is a full-stack e-commerce solution built with modern technologies, following Clean Architecture principles on the backend and a component-driven approach on the frontend.

## 🏗️ Project Structure

This repository uses a monorepo-style structure:
- **[VNVTStore.Backend](VNVTStore.Backend)**: .NET 8 Web API (Clean Architecture)
- **[VNVTStore.Frontend](VNVTStore.Frontend)**: React 18 + Vite + TypeScript (Modern UI)

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### 1. Backend Setup (`/VNVTStore.Backend`)
```bash
cd VNVTStore.Backend
# Ensure connection string is correct in src/VNVTStore.API/appsettings.Development.json
dotnet restore
dotnet build src/VNVTStore.API/VNVTStore.API.csproj
dotnet run --project src/VNVTStore.API/VNVTStore.API.csproj
```
API Documentation (Swagger) will be available at `http://localhost:5178/swagger` (or port configured in launchSettings).

### 2. Frontend Setup (`/VNVTStore.Frontend`)
```bash
cd VNVTStore.Frontend
npm install
npm run dev
```
Frontend will be available at `http://localhost:5173`.

## 🧪 Testing

### Backend Unit Tests
We strictly test our handlers using xUnit and Moq.
```bash
cd VNVTStore.Backend
dotnet test src/VNVTStore.Tests/VNVTStore.Tests.csproj
```

### Frontend Tests
```bash
cd VNVTStore.Frontend
npm test
```

## 📜 Features

- **Authentication**: JWT-based secure login and registration.
- **Product Catalog**: Dynamic product listing with category filtering and search.
- **Shopping Cart**: Real-time cart management synced with the backend.
- **Checkout Process**: Multi-step checkout with address management and order summary.
- **User Dashboard**: Manage profile, addresses, orders, and **Quote Requests**.
- **Admin Panel**: Manage products, categories, orders, customers, and system statistics (Charts included).
- **Quote System**: Request specialized price quotes for products directly from the platform.

## 🛠️ Technology Stack

| Part | Technologies |
|------|--------------|
| **Backend** | .NET 8, EF Core, PostgreSQL, MediatR, AutoMapper, FluentValidation, Asp.Versioning, xUnit, Moq |
| **Frontend** | React 19, Vite, TypeScript, Tailwind CSS 4, Zustand, React Query, React Hook Form, Zod, Framer Motion, Recharts, i18next |
| **Patterns** | Clean Architecture, CQRS, Repository, Unit of Work, Feature-Sliced Design (Frontend) |
| **DevOps** | GitHub Actions (CI/CD), Docker Support |

---
Managed by [vongovantien](https://github.com/vongovantien).
