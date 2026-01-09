import { Suspense, lazy } from 'react';
import { createBrowserRouter, RouterProvider, Outlet } from 'react-router-dom';

// Layouts
import { ShopLayout } from '@/layouts/ShopLayout';
import { AdminLayout } from '@/layouts/AdminLayout';

// Loading component
const PageLoader = () => (
  <div className="flex items-center justify-center min-h-screen">
    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
  </div>
);

// Lazy load pages for code splitting
const HomePage = lazy(() => import('@/pages/shop/HomePage'));
const ProductsPage = lazy(() => import('@/pages/shop/ProductsPage'));
const ProductDetailPage = lazy(() => import('@/pages/shop/ProductDetailPage'));
const CartPage = lazy(() => import('@/pages/shop/CartPage'));
const CheckoutPage = lazy(() => import('@/pages/shop/CheckoutPage'));
const QuoteRequestPage = lazy(() => import('@/pages/shop/QuoteRequestPage'));
const WishlistPage = lazy(() => import('@/pages/shop/WishlistPage'));
const ComparePage = lazy(() => import('@/pages/shop/ComparePage'));
const LoginPage = lazy(() => import('@/pages/auth/LoginPage'));
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage'));
const AccountPage = lazy(() => import('@/pages/shop/AccountPage'));
const PromotionsPage = lazy(() => import('@/pages/shop/PromotionsPage'));
const NewsPage = lazy(() => import('@/pages/shop/NewsPage'));
const ContactPage = lazy(() => import('@/pages/shop/ContactPage'));
const AboutPage = lazy(() => import('@/pages/shop/AboutPage'));
const SupportPage = lazy(() => import('@/pages/shop/SupportPage'));
const TrackingPage = lazy(() => import('@/pages/shop/TrackingPage'));

// Admin pages
const AdminDashboard = lazy(() => import('@/pages/admin/DashboardPage'));
const AdminProducts = lazy(() => import('@/pages/admin/ProductsPage'));
const AdminOrders = lazy(() => import('@/pages/admin/OrdersPage'));
const AdminCustomers = lazy(() => import('@/pages/admin/CustomersPage'));
const AdminQuotes = lazy(() => import('@/pages/admin/QuotesPage'));
const AdminSettings = lazy(() => import('@/pages/admin/SettingsPage'));
const AdminCategories = lazy(() => import('@/pages/admin/CategoriesPage'));
const AdminSuppliers = lazy(() => import('@/pages/admin/SuppliersPage'));

// Error pages
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'));

// Router configuration
const router = createBrowserRouter([
  // Shop routes
  {
    path: '/',
    element: <ShopLayout />,
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<PageLoader />}>
            <HomePage />
          </Suspense>
        ),
      },
      {
        path: 'products',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ProductsPage />
          </Suspense>
        ),
      },
      {
        path: 'product/:id',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ProductDetailPage />
          </Suspense>
        ),
      },
      {
        path: 'cart',
        element: (
          <Suspense fallback={<PageLoader />}>
            <CartPage />
          </Suspense>
        ),
      },
      {
        path: 'checkout',
        element: (
          <Suspense fallback={<PageLoader />}>
            <CheckoutPage />
          </Suspense>
        ),
      },
      {
        path: 'quote-request/:productId',
        element: (
          <Suspense fallback={<PageLoader />}>
            <QuoteRequestPage />
          </Suspense>
        ),
      },
      {
        path: 'wishlist',
        element: (
          <Suspense fallback={<PageLoader />}>
            <WishlistPage />
          </Suspense>
        ),
      },
      {
        path: 'compare',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ComparePage />
          </Suspense>
        ),
      },
      {
        path: 'promotions',
        element: (
          <Suspense fallback={<PageLoader />}>
            <PromotionsPage />
          </Suspense>
        ),
      },
      {
        path: 'news',
        element: (
          <Suspense fallback={<PageLoader />}>
            <NewsPage />
          </Suspense>
        ),
      },
      {
        path: 'contact',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ContactPage />
          </Suspense>
        ),
      },
      {
        path: 'account/*',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AccountPage />
          </Suspense>
        ),
      },
    ],
  },

  // Auth routes
  {
    path: '/login',
    element: (
      <Suspense fallback={<PageLoader />}>
        <LoginPage />
      </Suspense>
    ),
  },
  {
    path: '/register',
    element: (
      <Suspense fallback={<PageLoader />}>
        <RegisterPage />
      </Suspense>
    ),
  },

  // Admin routes
  {
    path: '/admin',
    element: <AdminLayout />,
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminDashboard />
          </Suspense>
        ),
      },
      {
        path: 'products',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminProducts />
          </Suspense>
        ),
      },
      {
        path: 'orders',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminOrders />
          </Suspense>
        ),
      },
      {
        path: 'customers',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminCustomers />
          </Suspense>
        ),
      },
      {
        path: 'quotes',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminQuotes />
          </Suspense>
        ),
      },
      {
        path: 'settings',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminSettings />
          </Suspense>
        ),
      },
      {
        path: 'categories',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminCategories />
          </Suspense>
        ),
      },
      {
        path: 'suppliers',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AdminSuppliers />
          </Suspense>
        ),
      },
    ],
  },

  // 404 Not Found - catch all unmatched routes
  {
    path: '*',
    element: (
      <Suspense fallback={<PageLoader />}>
        <NotFoundPage />
      </Suspense>
    ),
  },
]);

export const AppRouter = () => <RouterProvider router={router} />;

export default AppRouter;
