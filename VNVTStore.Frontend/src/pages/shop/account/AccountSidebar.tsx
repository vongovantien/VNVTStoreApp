import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  User,
  ShoppingBag,
  Heart,
  MapPin,
  Bell,
  Settings,
  LogOut,
} from 'lucide-react';
import { useAuthStore } from '@/store';
import { ConfirmDialog } from '@/components/ui';
import { useState } from 'react';

const AccountSidebar = () => {
  const { t } = useTranslation();
  const { logout, user } = useAuthStore();
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);

  const confirmLogout = () => {
    logout();
    setShowLogoutConfirm(false);
    window.location.href = '/';
  };

  const menuItems = [
    { path: '/account', icon: User, label: t('common.account.profile'), end: true },
    { path: '/account/quotes', icon: ShoppingBag, label: t('common.account.quotes') || t('admin.sidebar.quotes') }, 
    { path: '/account/orders', icon: ShoppingBag, label: t('common.account.orders') },
    { path: '/account/wishlist', icon: Heart, label: t('common.wishlist') },
    { path: '/account/addresses', icon: MapPin, label: t('common.account.addresses') },
    { path: '/account/notifications', icon: Bell, label: t('common.account.notificationSettings') },
    { path: '/account/settings', icon: Settings, label: t('admin.sidebar.settings') },
  ];

  return (
    <aside className="w-full lg:w-72 flex-shrink-0 lg:sticky lg:top-24 self-start">
      <div className="bg-primary rounded-2xl p-5 border border-secondary/10 shadow-sm">
        {/* User Info */}
        <div className="flex items-center gap-4 pb-6 mb-6 border-b border-secondary/10">
          <div className="w-14 h-14 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-indigo-200 dark:shadow-none">
            {user?.fullName?.charAt(0) || user?.username?.charAt(0) || 'U'}
          </div>
          <div className="flex-1 min-w-0 text-left">
            <p className="font-bold text-primary truncate text-lg">{user?.fullName || t('common.types.customer')}</p>
            <p className="text-sm text-tertiary truncate">{user?.email || 'email@example.com'}</p>
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
                `flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-200 text-sm font-medium ${
                  isActive 
                    ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-200 dark:shadow-indigo-900/40' 
                    : 'text-secondary hover:bg-secondary/50 hover:text-primary'
                }`
              }
            >
              <item.icon size={18} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* Logout */}
        <button
          onClick={() => setShowLogoutConfirm(true)}
          className="flex items-center gap-3 px-4 py-3 w-full mt-6 pt-6 border-t border-secondary/10 text-error hover:bg-error/10 rounded-xl transition-all duration-200 font-medium"
        >
          <LogOut size={18} />
          <span>{t('common.logout')}</span>
        </button>
      </div>
      <ConfirmDialog
        isOpen={showLogoutConfirm}
        onClose={() => setShowLogoutConfirm(false)}
        onConfirm={confirmLogout}
        title={t('common.logout')}
        message={t('messages.logoutConfirmMessage') || 'Are you sure you want to log out?'}
        confirmText={t('common.logout')}
        cancelText={t('common.cancel')}
      />
    </aside>
  );
};

export default AccountSidebar;
