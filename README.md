# 🛍️ VNVTStore - Modern Full-Stack E-Commerce Platform

[![CI/CD Pipeline](https://github.com/vongovantien/VNVTStoreApp/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/vongovantien/VNVTStoreApp/actions)
[![Backend](https://img.shields.io/badge/.NET-8.0-blueviolet)](VNVTStore.Backend)
[![Frontend](https://img.shields.io/badge/React-19-61dafb)](VNVTStore.Frontend)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

VNVTStore is a state-of-the-art e-commerce ecosystem designed for performance, scalability, and developer experience. It features a robust **Clean Architecture** backend and a high-performance **Vite-powered** frontend.

---

## 🏗️ System Architecture

Our solution follows a **Monorepo** structure, ensuring atomic coordination between the API and the User Interface.

### [🚀 VNVTStore.Backend](VNVTStore.Backend)
A high-performance API built with **.NET 8** and **PostgreSQL**.
- **Patterns**: Clean Architecture, CQRS (MediatR), Repository & Unit of Work.
- **Domain Logic**: Rich Domain Models with encapsulated business rules (DDD).
- **Security**: JWT-based Authentication with sliding window Refresh Tokens.

### [🎨 VNVTStore.Frontend](VNVTStore.Frontend)
A premium user experience built with **React 19** and **Vite 7**.
- **Styling**: Tailwind CSS 4 with custom design tokens and glassmorphism.
- **State**: Lightweight stores using **Zustand**.
- **Data Flow**: Custom Axios service with automated **Interceptor-based** token refresh.
- **Micro-animations**: Powered by **Framer Motion** for a fluid UX.

---

## 🛠️ Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Core Backend** | .NET 8, EF Core 8, MediatR, FluentValidation, AutoMapper |
| **Persistence** | PostgreSQL |
| **Core Frontend** | React 19, TypeScript, Vite 7 |
| **Styling & UI** | Tailwind CSS 4, Lucide Icons, Framer Motion |
| **Testing** | xUnit, Moq (Backend) / Vitest, Playwright (Frontend) |
| **Deployment** | GitHub Actions, Render (BE), Vercel (FE) |

---

## 🚀 Quick Start Guide

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (optional for local database)

### 2. Launch Backend
```bash
cd VNVTStore.Backend
# Configure connection string in appsettings.json
dotnet run --project src/VNVTStore.API/VNVTStore.API.csproj
```
> [!TIP]
> Access Swagger UI at `http://localhost:5178/swagger` for API documentation.

### 3. Launch Frontend
```bash
cd VNVTStore.Frontend
npm install
npm run dev
```
> [!NOTE]
> The app will be available at `http://localhost:5173`.

---

## 🔄 Automated CI/CD Pipeline

We utilize **GitHub Actions** to enforce code quality and automate global deployment.

- **Trigger**: Every push to the `dev` or `main` branches.
- **Stages**:
  - **Linting & Testing**: Running 130+ backend tests and the full vitest suite for frontend.
  - **Build**: Compiling optimized Release binaries and production React bundles.
  - **Multi-Cloud Deploy**: Automatic rollout to **Render** (API) and **Vercel** (UI).

---

## 📬 Contact & Support

- **Author**: [vongovantien](https://github.com/vongovantien)
- **Email**: vongovantien@gmail.com
- **Repository**: [VNVTStoreApp](https://github.com/vongovantien/VNVTStoreApp)

---
<p align="center">© 2026 VNVTStore. All Rights Reserved.</p>
