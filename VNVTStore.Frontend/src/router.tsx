import { Suspense, lazy } from 'react';
import { createBrowserRouter, RouterProvider, Outlet } from 'react-router-dom';

// Layouts
import { ShopLayout } from '@/layouts/ShopLayout';
import { AdminLayout } from '@/layouts/AdminLayout';

// Loading component
import { PageLoader } from '@/components/common/PageLoader';

// Lazy load pages for code splitting
const HomePage = lazy(() => import('@/pages/shop/HomePage'));
const ProductsPage = lazy(() => import('@/pages/shop/ProductsPage'));
const ProductDetailPage = lazy(() => import('@/pages/shop/ProductDetailPage'));
const CartPage = lazy(() => import('@/pages/shop/CartPage'));
const CheckoutPage = lazy(() => import('@/pages/shop/CheckoutPage'));
const OrderSuccessPage = lazy(() => import('@/pages/shop/OrderSuccessPage'));
const VerifyOrderPage = lazy(() => import('@/pages/shop/VerifyOrderPage'));
const QuoteRequestPage = lazy(() => import('@/pages/shop/QuoteRequestPage'));
const WishlistPage = lazy(() => import('@/pages/shop/WishlistPage'));
const ComparePage = lazy(() => import('@/pages/shop/ComparePage'));
const LoginPage = lazy(() => import('@/pages/auth/LoginPage'));
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage'));
const VerifyEmailPage = lazy(() => import('@/pages/auth/VerifyEmailPage'));
const ForgotPasswordPage = lazy(() => import('@/pages/auth/ForgotPasswordPage'));
const ResetPasswordPage = lazy(() => import('@/pages/auth/ResetPasswordPage'));
const AccountPage = lazy(() => import('@/pages/shop/AccountPage'));
const PromotionsPage = lazy(() => import('@/pages/shop/PromotionsPage'));
const NewsPage = lazy(() => import('@/pages/shop/NewsPage'));
const NewsDetailPage = lazy(() => import('@/pages/shop/NewsDetailPage'));
const ContactPage = lazy(() => import('@/pages/shop/ContactPage'));
const AboutPage = lazy(() => import('@/pages/shop/AboutPage'));
const SupportPage = lazy(() => import('@/pages/shop/SupportPage'));
const TrackingPage = lazy(() => import('@/pages/shop/TrackingPage'));

// Admin pages
const AdminDashboard = lazy(() => import('@/pages/admin/DashboardPage'));
const AdminProducts = lazy(() => import('@/pages/admin/ProductsPage'));
const AdminPromotionsPage = lazy(() => import('@/pages/admin/PromotionsPage'));
const AdminOrders = lazy(() => import('@/pages/admin/OrdersPage'));
const AdminCustomers = lazy(() => import('@/pages/admin/CustomersPage'));
const AdminQuotes = lazy(() => import('@/pages/admin/QuotesPage'));
const AdminSettings = lazy(() => import('@/pages/admin/SettingsPage'));
const AdminCategories = lazy(() => import('@/pages/admin/CategoriesPage'));
const AdminSuppliers = lazy(() => import('@/pages/admin/SuppliersPage'));
const AdminBrands = lazy(() => import('@/pages/admin/BrandsPage'));
const AdminUnits = lazy(() => import('@/pages/admin/UnitsPage'));
const AdminBanners = lazy(() => import('@/pages/admin/BannersPage'));

// Error pages
import ErrorBoundary from '@/components/common/ErrorBoundary';
import ProtectedRoute from '@/components/common/ProtectedRoute';
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'));

// Router configuration
const router = createBrowserRouter([
  // Shop routes
  {
    path: '/',
    element: <ShopLayout />,
    errorElement: <ErrorBoundary />,
    children: [
      {
        errorElement: <ErrorBoundary />,
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
            path: 'order-success',
            element: (
              <Suspense fallback={<PageLoader />}>
                <OrderSuccessPage />
              </Suspense>
            ),
          },
          {
            path: 'verify-order',
            element: (
              <Suspense fallback={<PageLoader />}>
                <VerifyOrderPage />
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
            path: 'news/:id',
            element: (
              <Suspense fallback={<PageLoader />}>
                <NewsDetailPage />
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
            path: 'about',
            element: (
              <Suspense fallback={<PageLoader />}>
                <AboutPage />
              </Suspense>
            ),
          },
          {
            path: 'support',
            element: (
              <Suspense fallback={<PageLoader />}>
                <SupportPage />
              </Suspense>
            ),
          },
          {
            path: 'tracking',
            element: (
              <Suspense fallback={<PageLoader />}>
                <TrackingPage />
              </Suspense>
            ),
          },
          {
            path: 'account/*',
            element: (
              <ProtectedRoute>
                <Suspense fallback={<PageLoader />}>
                   <AccountPage />
                </Suspense>
              </ProtectedRoute>
            ),
          },
        ],
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
    errorElement: <ErrorBoundary />,
  },
  {
    path: '/register',
    element: (
      <Suspense fallback={<PageLoader />}>
        <RegisterPage />
      </Suspense>
    ),
    errorElement: <ErrorBoundary />,
  },
  {
    path: '/verify-email',
    element: (
      <Suspense fallback={<PageLoader />}>
        <VerifyEmailPage />
      </Suspense>
    ),
    errorElement: <ErrorBoundary />,
  },
  {
    path: '/forgot-password',
    element: (
      <Suspense fallback={<PageLoader />}>
        <ForgotPasswordPage />
      </Suspense>
    ),
    errorElement: <ErrorBoundary />,
  },
  {
    path: '/reset-password',
    element: (
      <Suspense fallback={<PageLoader />}>
        <ResetPasswordPage />
      </Suspense>
    ),
    errorElement: <ErrorBoundary />,
  },

  // Admin routes
  {
    path: '/admin',
    element: <AdminLayout />,
    errorElement: <ErrorBoundary />,
    children: [
      {
        errorElement: <ErrorBoundary />,
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
            path: 'promotions',
            element: (
              <Suspense fallback={<PageLoader />}>
                <AdminPromotionsPage />
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
          {
            path: 'brands',
            element: (
              <Suspense fallback={<PageLoader />}>
                <AdminBrands />
              </Suspense>
            ),
          },
          {
            path: 'units',
            element: (
              <Suspense fallback={<PageLoader />}>
                <AdminUnits />
              </Suspense>
            ),
          },
          {
            path: 'banners',
            element: (
              <Suspense fallback={<PageLoader />}>
                <AdminBanners />
              </Suspense>
            ),
          },
        ],
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
