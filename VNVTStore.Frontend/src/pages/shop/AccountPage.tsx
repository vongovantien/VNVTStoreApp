import { Suspense, lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import AccountSidebar from './account/AccountSidebar';
import { Loader2 } from 'lucide-react';

// Lazy load content components
const ProfileContent = lazy(() => import('./account/ProfileContent'));
const QuotesContent = lazy(() => import('./account/QuotesContent'));
const OrdersContent = lazy(() => import('./account/OrdersContent'));

const AddressesContent = lazy(() => import('./account/AddressesContent'));
const NotificationsContent = lazy(() => import('./account/OtherContent').then(module => ({ default: module.NotificationsContent })));
const SettingsContent = lazy(() => import('./account/OtherContent').then(module => ({ default: module.SettingsContent })));

const ContentLoader = () => (
  <div className="flex items-center justify-center py-12">
    <Loader2 className="w-8 h-8 animate-spin text-primary" />
  </div>
);

// ============ Main Account Page ============
export const AccountPage = () => {
  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col lg:flex-row gap-8">
          <AccountSidebar />
          
          <main className="flex-1">
            <Suspense fallback={<ContentLoader />}>
              <Routes>
                <Route index element={<ProfileContent />} />
                <Route path="quotes" element={<QuotesContent />} />
                <Route path="orders" element={<OrdersContent />} />

                <Route path="wishlist" element={<Navigate to="/wishlist" replace />} />
                <Route path="addresses" element={<AddressesContent />} />
                <Route path="notifications" element={<NotificationsContent />} />
                <Route path="settings" element={<SettingsContent />} />
              </Routes>
            </Suspense>
          </main>
        </div>
      </div>
    </div>
  );
};

export default AccountPage;
