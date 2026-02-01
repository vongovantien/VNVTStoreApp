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
    { path: '/account', icon: User, label: t('account.profile'), end: true },
    { path: '/account/quotes', icon: ShoppingBag, label: t('account.quotes') }, 
    { path: '/account/orders', icon: ShoppingBag, label: t('account.orders') },

    { path: '/account/wishlist', icon: Heart, label: t('account.wishlist') },
    { path: '/account/addresses', icon: MapPin, label: t('account.addresses') },
    { path: '/account/notifications', icon: Bell, label: t('account.notifications') },
    { path: '/account/settings', icon: Settings, label: t('account.settings') },
  ];

  return (
    <aside className="w-full lg:w-64 flex-shrink-0 lg:sticky lg:top-24 self-start">
      <div className="bg-primary rounded-xl p-4">
        {/* User Info */}
        <div className="flex items-center gap-3 pb-4 mb-4 border-b">
          <div className="w-12 h-12 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center text-white font-bold text-lg">
            {user?.fullName?.charAt(0) || 'U'}
          </div>
          <div>
            <p className="font-semibold">{user?.fullName || t('account.customer')}</p>
            <p className="text-sm text-tertiary">{user?.email || 'email@example.com'}</p>
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
          onClick={() => setShowLogoutConfirm(true)}
          className="flex items-center gap-3 px-4 py-3 w-full mt-4 pt-4 border-t text-error hover:bg-error/10 rounded-lg transition-colors"
        >
          <LogOut size={20} />
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
