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
import { UserAvatar } from '@/components/common';
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
    <aside className="w-full lg:w-80 flex-shrink-0 lg:sticky lg:top-28 self-start animate-fade-in">
      <div className="bg-primary rounded-[2rem] p-6 border border-secondary/10 shadow-xl shadow-indigo-500/5 overflow-hidden relative">
        {/* Decorative background element */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-accent/5 rounded-full -mr-16 -mt-16 blur-3xl pointer-events-none" />
        
        {/* User Info */}
        <div className="relative flex flex-col items-center gap-4 pb-8 mb-8 border-b border-secondary/5 text-center">
          <div className="relative">
            <UserAvatar 
              size="lg" 
              className="w-24 h-24 ring-4 ring-accent/20 border-4 border-primary shadow-2xl" 
            />
            <div className="absolute bottom-1 right-1 w-6 h-6 bg-emerald-500 border-4 border-primary rounded-full shadow-lg" title="Online" />
          </div>
          <div className="space-y-1 w-full px-2">
            <h3 className="font-black text-primary truncate text-xl tracking-tight leading-tight">
                {user?.fullName || t('common.types.customer')}
            </h3>
            <p className="text-xs font-semibold text-tertiary truncate uppercase tracking-widest opacity-80">
                {user?.email || 'email@example.com'}
            </p>
            {user && user.role === 'Customer' && (
              <div className="mt-3 flex flex-col items-center gap-2 animate-in fade-in slide-in-from-bottom-1 duration-500">
                <div className="flex items-center gap-2 px-3 py-1 bg-accent/10 rounded-full border border-accent/20">
                  <span className="text-base">✨</span>
                  <span className="text-sm font-black text-accent">{user.loyaltyPoints || 0} {t('membership.points')}</span>
                </div>
                {/* Membership Badge */}
                <div className={`px-4 py-1 rounded-lg text-[10px] font-black uppercase tracking-tighter shadow-sm border ${
                  (user.loyaltyPoints || 0) >= 5000 ? 'bg-amber-400 text-amber-950 border-amber-500 shadow-amber-500/20' :
                  (user.loyaltyPoints || 0) >= 1000 ? 'bg-slate-300 text-slate-800 border-slate-400 shadow-slate-400/20' :
                  'bg-orange-200 text-orange-900 border-orange-300 shadow-orange-300/20'
                }`}>
                  {(user.loyaltyPoints || 0) >= 5000 ? t('membership.gold', 'Thành viên Vàng') :
                   (user.loyaltyPoints || 0) >= 1000 ? t('membership.silver', 'Thành viên Bạc') :
                   t('membership.bronze', 'Thành viên Đồng')}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Menu */}
        <nav className="space-y-2 relative">
          {menuItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.end}
              className={({ isActive }) => 
                `group flex items-center gap-4 px-5 py-3.5 rounded-2xl transition-all duration-300 text-sm font-bold tracking-tight ${
                  isActive 
                    ? 'bg-accent text-white shadow-lg shadow-accent/40 translate-x-1' 
                    : 'text-secondary hover:bg-hover hover:text-accent hover:translate-x-1'
                }`
              }
            >
              <div className="transition-transform group-hover:scale-110">
                <item.icon size={20} />
              </div>
              <span className="flex-1 text-left">{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* Logout */}
        <button
          onClick={() => setShowLogoutConfirm(true)}
          className="group flex items-center gap-4 px-5 py-3.5 w-full mt-8 pt-8 border-t border-secondary/5 text-error/80 hover:text-error hover:bg-error/5 rounded-2xl transition-all duration-300 text-sm font-bold tracking-tight"
        >
          <div className="transition-transform group-hover:rotate-12">
            <LogOut size={20} />
          </div>
          <span className="text-left">{t('common.logout')}</span>
        </button>
      </div>
      <ConfirmDialog
        isOpen={showLogoutConfirm}
        onClose={() => setShowLogoutConfirm(false)}
        onConfirm={confirmLogout}
        title={t('common.logout')}
        message={t('common.messages.logoutConfirmMessage') || 'Are you sure you want to log out?'}
        confirmText={t('common.logout')}
        cancelText={t('common.cancel')}
      />
    </aside>
  );
};

export default AccountSidebar;
