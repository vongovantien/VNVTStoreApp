# VNVTStore - Modern E-Commerce Platform

VNVTStore is a full-stack e-commerce solution built with modern technologies, following Clean Architecture principles on the backend and a component-driven approach on the frontend.

## 🏗️ Project Structure

This repository contains:
- **/VNVTStore**: .NET 8 Web API (Backend)
- **/vnvt-react-app**: React 18 + Vite + TypeScript (Frontend)

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### 1. Backend Setup
```bash
cd VNVTStore
# Update connection string in src/VNVTStore.API/appsettings.Development.json
dotnet build
dotnet run --project src/VNVTStore.API
```
Backend will be available at `http://localhost:5176` (or as configured in `launchSettings.json`).

### 2. Frontend Setup
```bash
cd vnvt-react-app
npm install
npm run dev
```
Frontend will be available at `http://localhost:5173`.

## 📜 Features

- **Authentication**: JWT-based secure login and registration.
- **Product Catalog**: Dynamic product listing with category filtering and search.
- **Shopping Cart**: Real-time cart management synced with the backend.
- **Checkout Process**: Multi-step checkout with address management and order summary.
- **User Dashboard**: Manage profile, addresses, orders, and **Quote Requests**.
- **Admin Panel**: Manage products, categories, orders, and system statistics.
- **Quote System**: Request specialized price quotes for products directly from the platform.

## 🛠️ Technology Stack

| Part | Technologies |
|------|--------------|
| **Backend** | .NET 8, EF Core, PostgreSQL, MediatR, AutoMapper, FluentValidation |
| **Frontend** | React 18, Vite, TypeScript, Tailwind CSS, Lucide React, Axios |
| **Patterns** | Clean Architecture, CQRS, Repository, Unit of Work |

---
Managed by [vongovantien](https://github.com/vongovantien).
