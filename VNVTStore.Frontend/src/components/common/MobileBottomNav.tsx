/**
 * Feature #72: Bottom Navigation Bar (Mobile)
 * Feature #74: Haptic Feedback on tap
 */
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Home, Search, ShoppingCart, Heart, User } from 'lucide-react';
import { useCartStore, useWishlistStore } from '@/store';

const navItems = [
  { icon: Home, path: '/', label: 'common.home' },
  { icon: Search, path: '/products', label: 'common.search' },
  { icon: ShoppingCart, path: '/cart', label: 'common.cart', badge: 'cart' },
  { icon: Heart, path: '/wishlist', label: 'common.wishlist', badge: 'wishlist' },
  { icon: User, path: '/account', label: 'common.account' },
  { icon: User, path: '/account', label: 'common.account' },
];

export const MobileBottomNav = () => {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const cartCount = useCartStore((s) => s.items.length);
  const wishlistCount = useWishlistStore((s) => s.items.length);

  const handleTap = (path: string) => {
    // Feature #74: Haptic feedback
    if (navigator.vibrate) {
      navigator.vibrate(10);
    }
    navigate(path);
  };

  const getBadgeCount = (badge?: string) => {
    if (badge === 'cart') return cartCount;
    if (badge === 'wishlist') return wishlistCount;
    return 0;
  };

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 bg-primary/95 backdrop-blur-xl border-t shadow-2xl lg:hidden safe-area-bottom">
      <div className="flex items-center justify-around h-16 px-2">
        {navItems.map(({ icon: NavIcon, path, label, badge }) => {
          const isActive = location.pathname === path || 
            (path !== '/' && location.pathname.startsWith(path));
          const count = getBadgeCount(badge);

          return (
            <button
              key={path}
              onClick={() => handleTap(path)}
              className={`relative flex flex-col items-center justify-center gap-0.5 flex-1 py-1 transition-all duration-200 ${
                isActive 
                  ? 'text-accent-primary scale-105' 
                  : 'text-tertiary hover:text-primary'
              }`}
            >
              <div className="relative">
                <NavIcon size={22} strokeWidth={isActive ? 2.5 : 1.5} />
                {count > 0 && (
                  <span className="absolute -top-2 -right-2.5 min-w-[18px] h-[18px] flex items-center justify-center text-[10px] font-bold text-white bg-error rounded-full px-1 animate-bounce">
                    {count > 99 ? '99+' : count}
                  </span>
                )}
              </div>
              <span className={`text-[10px] font-medium ${isActive ? 'font-semibold' : ''}`}>
                {t(label)}
              </span>
              {isActive && (
                <div className="absolute top-0 left-1/2 -translate-x-1/2 w-8 h-0.5 bg-accent-primary rounded-full" />
              )}
            </button>
          );
        })}
      </div>
    </nav>
  );
};
