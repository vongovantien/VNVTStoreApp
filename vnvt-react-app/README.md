# VNVTStore Frontend (React App)

The modern React-based user interface for the VNVTStore E-Commerce platform.

## 🛠️ Tech Stack

- **React 18**: UI Framework
- **TypeScript**: Static typing for reliability
- **Vite**: Ultra-fast build tool and dev server
- **Tailwind CSS**: Modern utility-first styling
- **Lucide React**: Beautiful icons
- **Axios**: API communication with automated request interceptors
- **Zustand**: Lightweight state management (Cart, Auth)
- **i18next**: Multi-language support (English/Vietnamese)

## 📁 Folder Structure

- `/src/components`: Reusable UI components (Common, Layouts, Shop)
- `/src/pages`: Main application pages (Home, Product, Cart, Account, etc.)
- `/src/services`: API client and data fetching services
- `/src/stores`: Global state management
- `/src/hooks`: Custom React hooks
- `/src/types`: TypeScript interfaces and types
- `/src/utils`: Helper functions and formatting utilities
- `/src/locales`: Translation files

## 🚀 Getting Started

1. **Install dependencies**:
   ```bash
   npm install
   ```
2. **Configure Environment**:
   Create a `.env` file in the root (optional, default is http://localhost:5176):
   ```
   VITE_API_URL=http://localhost:5176/api/v1
   ```
3. **Run in development**:
   ```bash
   npm run dev
   ```

## 📜 Key Features Implemented

### 🛒 Shopping Experience
- Product listing with categories
- Dynamic product detail pages
- Side-nav cart and full cart management
- Multi-step checkout process

### 👤 User Account
- Modern Profile Management
- Multiple Address management
- Order tracking and history
- **Quote Requests dashboard**

### 🎨 Design System
- Modern dark/light-friendly design
- Glassmorphism effects
- Responsive layout (Mobile/Tablet/Desktop)
- Smooth micro-animations

## 🧪 Testing
Run unit tests with Vitest:
```bash
npm run test
```
