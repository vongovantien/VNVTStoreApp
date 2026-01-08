import { useState } from 'react';
import { Routes, Route, NavLink, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  User,
  ShoppingBag,
  Heart,
  MapPin,
  Bell,
  Settings,
  LogOut,
  ChevronRight,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { useAuthStore } from '@/store';
import { mockOrders } from '@/data/mockData';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';

// ============ Account Sidebar ============
const AccountSidebar = () => {
  const { t } = useTranslation();
  const { logout } = useAuthStore();

  const menuItems = [
    { path: '/account', icon: User, label: t('account.profile'), end: true },
    { path: '/account/orders', icon: ShoppingBag, label: t('account.orders') },
    { path: '/account/wishlist', icon: Heart, label: t('account.wishlist') },
    { path: '/account/addresses', icon: MapPin, label: t('account.addresses') },
    { path: '/account/notifications', icon: Bell, label: t('account.notifications') },
    { path: '/account/settings', icon: Settings, label: t('account.settings') },
  ];

  return (
    <aside className="w-full lg:w-64 flex-shrink-0">
      <div className="bg-primary rounded-xl p-4">
        {/* User Info */}
        <div className="flex items-center gap-3 pb-4 mb-4 border-b">
          <div className="w-12 h-12 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center text-white font-bold text-lg">
            K
          </div>
          <div>
            <p className="font-semibold">Khách hàng demo</p>
            <p className="text-sm text-tertiary">customer@example.com</p>
          </div>
        </div>

        {/* Menu */}
        <nav className="space-y-1">
          {menuItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.end}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
                  isActive ? 'bg-primary/10 text-primary font-medium' : 'text-secondary hover:bg-secondary'
                }`
              }
            >
              <item.icon size={20} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* Logout */}
        <button
          onClick={logout}
          className="flex items-center gap-3 px-4 py-3 w-full mt-4 pt-4 border-t text-error hover:bg-error/10 rounded-lg transition-colors"
        >
          <LogOut size={20} />
          <span>{t('common.logout')}</span>
        </button>
      </div>
    </aside>
  );
};

// ============ Profile Page ============
const ProfileContent = () => {
  const { t } = useTranslation();
  
  return (
    <div className="bg-primary rounded-xl p-6">
      <h2 className="text-xl font-bold mb-6">{t('account.profile')}</h2>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium mb-2">Họ và tên</label>
          <input
            type="text"
            defaultValue="Khách hàng demo"
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-2">Email</label>
          <input
            type="email"
            defaultValue="customer@example.com"
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-2">Số điện thoại</label>
          <input
            type="tel"
            defaultValue="0901234567"
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-2">Ngày sinh</label>
          <input
            type="date"
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
      </div>

      <div className="mt-6">
        <Button>{t('common.save')}</Button>
      </div>
    </div>
  );
};

// ============ Orders Page ============
const OrdersContent = () => {
  const { t } = useTranslation();

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold">{t('account.orders')}</h2>

      {mockOrders.map((order) => (
        <div key={order.id} className="bg-primary rounded-xl p-4">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 mb-4">
            <div>
              <p className="font-semibold">{order.orderNumber}</p>
              <p className="text-sm text-tertiary">{formatDate(order.createdAt)}</p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold bg-${getStatusColor(order.status)}/20 text-${getStatusColor(order.status)}`}
            >
              {getStatusText(order.status)}
            </span>
          </div>

          <div className="space-y-2 mb-4">
            {order.items.map((item, index) => (
              <div key={index} className="flex items-center gap-3">
                <img
                  src={item.productImage}
                  alt={item.productName}
                  className="w-12 h-12 object-cover rounded"
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{item.productName}</p>
                  <p className="text-xs text-tertiary">x{item.quantity}</p>
                </div>
                <p className="text-sm font-medium">{formatCurrency(item.price * item.quantity)}</p>
              </div>
            ))}
          </div>

          <div className="flex justify-between items-center pt-4 border-t">
            <span className="text-sm text-secondary">{t('cart.total')}</span>
            <span className="font-bold text-error">{formatCurrency(order.total)}</span>
          </div>
        </div>
      ))}
    </div>
  );
};

// ============ Placeholder Pages ============
const AddressesContent = () => (
  <div className="bg-primary rounded-xl p-6">
    <h2 className="text-xl font-bold mb-4">Địa chỉ giao hàng</h2>
    <p className="text-secondary">Chưa có địa chỉ nào được lưu.</p>
    <Button className="mt-4">Thêm địa chỉ mới</Button>
  </div>
);

const NotificationsContent = () => (
  <div className="bg-primary rounded-xl p-6">
    <h2 className="text-xl font-bold mb-4">Thông báo</h2>
    <p className="text-secondary">Không có thông báo mới.</p>
  </div>
);

const SettingsContent = () => (
  <div className="bg-primary rounded-xl p-6">
    <h2 className="text-xl font-bold mb-6">Cài đặt</h2>
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <span>Nhận email khuyến mãi</span>
        <input type="checkbox" defaultChecked className="w-5 h-5" />
      </div>
      <div className="flex items-center justify-between">
        <span>Nhận thông báo đơn hàng</span>
        <input type="checkbox" defaultChecked className="w-5 h-5" />
      </div>
    </div>
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
            <Routes>
              <Route index element={<ProfileContent />} />
              <Route path="orders" element={<OrdersContent />} />
              <Route path="wishlist" element={<Navigate to="/wishlist" replace />} />
              <Route path="addresses" element={<AddressesContent />} />
              <Route path="notifications" element={<NotificationsContent />} />
              <Route path="settings" element={<SettingsContent />} />
            </Routes>
          </main>
        </div>
      </div>
    </div>
  );
};

export default AccountPage;
