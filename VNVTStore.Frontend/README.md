# 🎨 VNVTStore Frontend - High-Performance React UI

[![Framework](https://img.shields.io/badge/React-19-blue?logo=react)](https://react.dev/)
[![Build Tool](https://img.shields.io/badge/Vite-7-646cff?logo=vite)](https://vitejs.dev/)
[![Styling](https://img.shields.io/badge/Tailwind-CSS%204-38bdf8?logo=tailwind-css)](https://tailwindcss.com/)
[![Testing](https://img.shields.io/badge/Tests-Vitest-yellow?logo=vitest)](https://vitest.dev/)

A premium, responsive, and high-performance user interface for the **VNVTStore** ecosystem. Built with **React 19** and optimized for lightning-fast delivery using **Vite 7**.

---

## 🛠️ Key Technical Features

### 🧩 Feature-Sliced Design (FSD)
The project structure is inspired by **Feature-Sliced Design**, ensuring clean separation between:
- **Shared**: Common UI elements (Button, Input, Badge).
- **Entities**: Business-specific models (Product, Cart, Order, Address).
- **Features**: Interactive user actions (Add To Cart, Apply Coupon, Search).
- **Pages**: Full-screen compositions.

### 🛡️ Secure Auto-Refresh Service
Our **Axios Service** includes a sophisticated recursive **Interceptor** system:
- **Auto-Retry**: If a request fails with 401, it automatically triggers a refresh token request and retries the original call.
- **Queue Management**: Prevents race conditions during multiple concurrent token refreshes.
- **Global Handling**: Unified error parsing and toast notification mapping.

### 🎨 Design & Experience
- **Glassmorphism**: Elegant card designs with backdrop blur and border gradients.
- **Dynamic Theming**: Support for both vibrant light and sleek dark themes.
- **Micro-Animations**: Fluid transitions using **Framer Motion**.
- **SEO Ready**: Dynamic meta tags and semantic HTML for superior search ranking.

---

## 📁 Project Structure

```text
src/
├── components/         # Atomic UI (Common, Layout, Shop)
├── services/           # API Clients & Data Services
├── stores/             # Global state management (Zustand)
├── hooks/              # Custom application hooks
├── pages/              # Route compositions
├── types/              # Unified TypeScript definitions
├── utils/              # Formatting & Helper functions
└── locales/            # Multi-language assets (EN/VI)
```

---

## 🚀 Development Workflow

### 1. Installation
```bash
npm install
```

### 2. Environment Configuration
Create a `.env` file at the root:
```env
VITE_API_URL=https://api.vnvtstore.com/v1
```

### 3. Run Development Server
```bash
npm run dev
```
> [!TIP]
> Visit `http://localhost:5173` to view the live app with HMR.

---

## 🧪 Testing Mastery
We maintain a zero-regression policy. Run our unified test suite:
```bash
# Unit Tests (Logic & Components)
npm run test

# Browser Tests (Playwright)
npx playwright test
```

---
<p align="center">Built with ❤️ by [vongovantien](https://github.com/vongovantien)</p>
