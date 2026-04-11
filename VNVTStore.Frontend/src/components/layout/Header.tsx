import { memo, useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Search,
  ShoppingCart,
  Heart,
  User,
  Menu,
  X,
  Sun,
  Moon,
  ChevronDown,
  Phone,
  MessageCircle,
  Scale,
  LayoutDashboard,
  Package,
  ShieldAlert,
  Loader2,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button, ConfirmDialog } from '@/components/ui';
import { useCartStore, useWishlistStore, useUIStore, useCompareStore, useAuthStore, useNotificationStore, useToast, useRecentStore } from '@/store';
import { NotificationDropdown, UserMenu, LanguageSwitcher } from '@/components/common';
import { type SignalRNotification } from '@/services/signalrService';
import { useClickOutside, useSignalR, useDebounce } from '@/hooks';
import { useCategories, useProducts } from '@/hooks/useProducts';
import { formatCurrency } from '@/utils/format';

export const Header = memo(() => {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showCategories, setShowCategories] = useState(false);
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);
  const [showSearchResults, setShowSearchResults] = useState(false);

  // Categories loading
  const { data: categoriesData, isLoading: isLoadingCategories } = useCategories({ 
    enabled: showCategories || mobileMenuOpen 
  });
  const categories = (categoriesData as any[]) || [];

  const debouncedSearch = useDebounce(searchQuery, 300);
  const searchRef = useClickOutside<HTMLDivElement>(() => setShowSearchResults(false));
  const categoriesRef = useClickOutside<HTMLDivElement>(() => setShowCategories(false));

  const { data: searchData, isLoading: isSearching } = useProducts({
    search: debouncedSearch,
    pageSize: 5,
    enabled: !!debouncedSearch && showSearchResults
  });

  const searchResults = searchData?.products || [];

  const handleLogoutClick = () => {
    setMobileMenuOpen(false);
    setShowLogoutConfirm(true);
  };

  const { user, isAuthenticated, logout, adminToken, stopImpersonating } = useAuthStore();
  const { theme, toggleTheme, setCartOpen } = useUIStore();
  const { addNotification } = useNotificationStore();
  const { info } = useToast();
  const { on, isConnected } = useSignalR();

  const cartCount = useCartStore((state) => state.getItemCount());
  const wishlistCount = useWishlistStore((state) => state.items.length);
  const compareCount = useCompareStore((state) => state.items.length);

  const confirmLogout = () => {
    logout();
    setShowLogoutConfirm(false);
    navigate('/');
  };

  useEffect(() => {
    if (!isConnected) return;

    const unsubscribeOrder = on('ReceiveOrderNotification', (data: string | SignalRNotification) => {
        const msg = typeof data === 'string' ? data : (data as SignalRNotification).Message || '';
        addNotification(msg);
        info(`${t('common.messages.newOrder')}: ${msg}`);
    });
    
    const unsubscribeQuote = on('ReceiveQuoteNotification', (data: string | unknown) => {
         const msg = typeof data === 'string' ? data : (data as { Message?: string }).Message || '';
         addNotification(msg);
         info(`${t('common.messages.newQuote')}: ${msg}`);
    });

    return () => {
        unsubscribeOrder();
        unsubscribeQuote();
    };
  }, [isConnected, on, addNotification, info, t]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      useRecentStore.getState().addSearchQuery(searchQuery.trim());
      navigate(`/search?search=${encodeURIComponent(searchQuery)}`);
      setShowSearchResults(false);
    }
  };

  return (
    <>
      {adminToken && (
        <div className="bg-amber-500 text-white py-1.5 px-4 text-center text-sm font-bold flex items-center justify-center gap-4 animate-in slide-in-from-top duration-300">
          <div className="flex items-center gap-2">
            <ShieldAlert size={16} />
            <span>{t('admin.impersonatingMessage', 'You are logged in as {{name}}', { name: user?.fullName || user?.username })}</span>
          </div>
          <button 
            onClick={() => {
                if (window.confirm(t('common.messages.confirmStopImpersonate'))) {
                    stopImpersonating();
                }
            }}
            className="bg-white/20 hover:bg-white/30 px-3 py-0.5 rounded-full transition-colors text-xs backdrop-blur-sm border border-white/30"
          >
            {t('admin.actions.stopImpersonation', 'Stop Impersonating')}
          </button>
        </div>
      )}
      <header className="bg-white dark:bg-slate-950 transition-all duration-300 relative z-[101]">
        {/* Top Bar - Refined */}
        <div className="bg-slate-900 dark:bg-black text-slate-400 text-[11px] py-2 border-b border-white/5">
          <div className="container mx-auto px-4 flex justify-between items-center tracking-wide font-medium">
            <div className="flex gap-8">
              <a href="tel:1900123456" className="flex items-center gap-2 hover:text-white transition-all duration-300 group">
                <Phone size={10} className="group-hover:scale-110 transition-transform" />
                <span>{t('shared.hotlineFull')}</span>
              </a>
              <a href="https://zalo.me" className="flex items-center gap-2 text-sky-400 hover:text-sky-300 transition-all duration-300 group">
                <MessageCircle size={10} className="group-hover:scale-110 transition-transform" />
                <span>{t('shared.chatZalo')}</span>
              </a>
            </div>
            <div className="hidden md:flex gap-6">
              <Link to="/tracking" className="hover:text-white transition-colors">{t('header.trackOrder')}</Link>
              <Link to="/support" className="hover:text-white transition-colors">{t('header.support')}</Link>
              <Link to="/about" className="hover:text-white transition-colors">{t('header.about')}</Link>
            </div>
          </div>
        </div>

        {/* Main Header - Clean & Spacious */}
        <div className="py-5 border-b border-slate-100 dark:border-slate-900">
          <div className="container mx-auto px-4 flex items-center gap-10">
            {/* Logo - Elegant */}
            <Link to="/" className="flex items-center gap-3 flex-shrink-0 group">
              <div className="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center text-white text-xl shadow-lg shadow-indigo-500/20 group-hover:rotate-6 transition-transform duration-300">
                <span>🏠</span>
              </div>
              <div className="flex flex-col">
                <span className="text-2xl font-black tracking-tighter bg-gradient-to-r from-indigo-600 to-violet-500 bg-clip-text text-transparent">
                  VNVT
                </span>
                <span className="text-[10px] uppercase tracking-[0.2em] text-slate-400 font-bold -mt-1">
                  Premium Store
                </span>
              </div>
            </Link>

            {/* Search Bar - Modern & Minimal */}
            <div className="flex-1 max-w-xl hidden md:block relative group">
              <form onSubmit={handleSearch} className="relative z-50">
                <div className="flex items-center bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 focus-within:border-indigo-500 focus-within:ring-4 focus-within:ring-indigo-500/10 rounded-2xl pl-5 pr-1.5 py-1.5 transition-all duration-300">
                  <Search size={18} className="text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
                  <div className="flex-1 ml-3 mr-2">
                    <input
                      type="text"
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      onFocus={() => setShowSearchResults(true)}
                      placeholder={t('header.searchPlaceholder')}
                      className="w-full bg-transparent text-[13px] !border-none !ring-0 !outline-none focus:!ring-0 focus:!border-none text-slate-900 dark:text-slate-100 placeholder:text-slate-400 shadow-none"
                    />
                  </div>
                  {searchQuery && (
                    <button 
                      type="button" 
                      onClick={() => setSearchQuery('')}
                      className="p-1.5 hover:bg-slate-200 dark:hover:bg-slate-800 rounded-lg mr-2 text-slate-400 transition-colors"
                    >
                      <X size={14} />
                    </button>
                  )}
                  <Button type="submit" size="sm" className="rounded-xl px-5 font-bold shadow-md shadow-indigo-500/20 active:scale-95 transition-transform">
                    {t('common.search')}
                  </Button>
                </div>
              </form>

              {/* Live Search Results Popover */}
              <AnimatePresence>
                {showSearchResults && debouncedSearch && (
                  <motion.div
                    ref={searchRef}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    className="absolute top-full left-0 right-0 mt-2 bg-primary rounded-2xl shadow-2xl border border-border overflow-hidden z-[110]"
                  >
                    <div className="p-4 border-b border-border flex justify-between items-center">
                      <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">
                        {t('header.searchMatches', 'Kết quả tìm kiếm')}
                      </span>
                      {isSearching && <Loader2 size={14} className="animate-spin text-indigo-500" />}
                    </div>

                    <div className="max-h-[400px] overflow-y-auto py-2">
                      {searchResults.length > 0 ? (
                        <>
                          {searchResults.map((p) => (
                            <Link
                              key={p.code}
                              to={`/product/${p.code}`}
                              onClick={() => setShowSearchResults(false)}
                              className="flex items-center gap-4 px-4 py-3 hover:bg-hover transition-colors group"
                            >
                              <div className="w-12 h-12 rounded-lg bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 overflow-hidden flex-shrink-0">
                                <img src={p.image} alt={p.name} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300" />
                              </div>
                              <div className="flex-1 min-w-0">
                                <h4 className="text-sm font-semibold text-slate-800 dark:text-slate-100 truncate group-hover:text-indigo-600 transition-colors">
                                  {p.name}
                                </h4>
                                <p className="text-xs text-slate-400 truncate">{p.category}</p>
                              </div>
                              <div className="text-right">
                                <div className="text-sm font-bold text-slate-900 dark:text-white">
                                  {p.price > 0 ? formatCurrency(p.price) : t('product.contact')}
                                </div>
                              </div>
                            </Link>
                          ))}
                          <Link
                            to={`/search?search=${encodeURIComponent(debouncedSearch)}`}
                            onClick={() => setShowSearchResults(false)}
                            className="block text-center py-3 text-xs font-bold text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 border-t border-slate-100 dark:border-slate-700 mt-2"
                          >
                            {t('header.viewAllResults', 'Xem tất cả kết quả cho "{{query}}"', { query: debouncedSearch })}
                          </Link>
                        </>
                      ) : !isSearching ? (
                        <div className="p-8 text-center text-slate-400 text-sm">
                          {t('header.noResults', 'Không tìm thấy sản phẩm nào')}
                        </div>
                      ) : null}
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            {/* Actions - Refined & Balanced */}
            <div className="flex items-center gap-3">
              <div className="hidden lg:flex items-center bg-slate-50 dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-2xl p-1 gap-1">
                <LanguageSwitcher variant="ghost" align="right" className="transition-all hover:bg-white dark:hover:bg-slate-800 rounded-xl" />
                
                <Button variant="ghost" size="sm" onClick={toggleTheme} className="w-9 h-9 p-0 rounded-xl hover:bg-white dark:hover:bg-slate-800 transition-all">
                  {theme === 'light' ? <Moon size={18} /> : <Sun size={18} />}
                </Button>
              </div>

              <div className="flex items-center gap-1.5 ml-2">
                {isAuthenticated && (
                  <NotificationDropdown 
                    isConnected={isConnected}
                    onNotificationClick={() => navigate('/account/notifications')}
                  />
                )}
                
                <Link to="/compare" className="hidden sm:flex group">
                  <Button variant="ghost" size="sm" className="w-10 h-10 p-0 rounded-xl relative hover:bg-indigo-50 dark:hover:bg-indigo-900/30">
                    <Scale size={20} className="text-slate-600 dark:text-slate-300 group-hover:text-indigo-500 transition-colors" />
                    {compareCount > 0 && (
                      <span className="absolute -top-1 -right-1 w-5 h-5 bg-indigo-600 text-white text-[10px] font-bold rounded-full flex items-center justify-center border-2 border-white dark:border-slate-950">
                        {compareCount}
                      </span>
                    )}
                  </Button>
                </Link>

                <Link to="/wishlist" className="hidden sm:flex group">
                  <Button variant="ghost" size="sm" className="w-10 h-10 p-0 rounded-xl relative hover:bg-rose-50 dark:hover:bg-rose-900/30">
                    <Heart size={20} className="text-slate-600 dark:text-slate-300 group-hover:text-rose-500 transition-colors" />
                    {wishlistCount > 0 && (
                      <span className="absolute -top-1 -right-1 w-5 h-5 bg-rose-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center border-2 border-white dark:border-slate-950">
                        {wishlistCount}
                      </span>
                    )}
                  </Button>
                </Link>

                <Button variant="ghost" size="sm" className="w-10 h-10 p-0 rounded-xl relative hover:bg-amber-50 dark:hover:bg-amber-900/30 group" onClick={() => setCartOpen(true)}>
                  <ShoppingCart size={20} className="text-slate-600 dark:text-slate-300 group-hover:text-amber-500 transition-colors" />
                  {cartCount > 0 && (
                    <span className="absolute -top-1 -right-1 w-5 h-5 bg-amber-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center border-2 border-white dark:border-slate-950">
                      {cartCount}
                    </span>
                  )}
                </Button>
              </div>

              <div className="h-8 w-[1px] bg-slate-100 dark:bg-slate-800 mx-1 hidden lg:block" />

              {isAuthenticated ? (
                <UserMenu 
                  className="hidden lg:block z-20"
                  onLogout={handleLogoutClick}
                  items={[
                    ...(user?.role === 'Admin' ? [{
                      label: t('common.adminDashboard'),
                      icon: LayoutDashboard,
                      link: '/admin',
                      className: 'text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-900/20'
                    }] : []),
                    { label: t('common.account.title'), icon: User, link: '/account' },
                    { label: t('common.account.orders'), icon: Package, link: '/account/orders' }
                  ]}
                />
              ) : (
                <div className="hidden lg:flex items-center gap-2 ml-2">
                  <Link to="/login">
                    <Button variant="ghost" size="sm" className="px-5 font-bold text-slate-600 dark:text-slate-300 hover:text-indigo-600">
                      {t('common.login')}
                    </Button>
                  </Link>
                  <Link to="/register">
                    <Button size="sm" className="px-6 font-bold rounded-xl shadow-lg shadow-indigo-500/20 active:scale-95 transition-transform">
                      {t('common.register')}
                    </Button>
                  </Link>
                </div>
              )}

              <Button variant="ghost" size="sm" className="lg:hidden w-10 h-10 p-0 rounded-xl" onClick={() => setMobileMenuOpen(true)}>
                <Menu size={24} />
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Navigation - Premium Glassmorphism */}
      <nav className="hidden lg:block border-b border-slate-100 dark:border-slate-900/50 bg-white/80 dark:bg-slate-950/80 backdrop-blur-md transition-all duration-300 sticky top-0 z-[100] shadow-sm">
        <div className="container mx-auto px-4 flex items-center gap-6">
          <div className="relative" ref={categoriesRef}>
            <Button
              onClick={() => setShowCategories(!showCategories)}
              className="rounded-none py-3"
              leftIcon={<Menu size={18} />}
              rightIcon={<ChevronDown size={16} />}
            >
              {t('header.categoryMenu')}
            </Button>
            {showCategories && (
              <div className="absolute top-full left-0 bg-primary rounded-b-xl shadow-xl border-t-0 p-4 w-[800px] max-w-[calc(100vw-2rem)] max-h-[75vh] overflow-y-auto z-[999] grid grid-cols-3 gap-6">
                {isLoadingCategories ? (
                   <div className="col-span-3 text-center py-8 text-secondary text-sm">Loading categories...</div>
                ) : (
                   categories.filter(c => !c.parentCode).map((parent) => (
                    <div key={parent.code} className="space-y-3">
                      <Link
                        to={`/products?category=${parent.code}`}
                        onClick={() => setShowCategories(false)}
                        className="font-bold text-base hover:text-indigo-600 block"
                      >
                        {parent.name}
                      </Link>
                      <ul className="space-y-2">
                        {categories.filter(c => c.parentCode === parent.code).map((child) => (
                          <li key={child.code}>
                            <Link
                              to={`/products?category=${child.code}`}
                              onClick={() => setShowCategories(false)}
                              className="text-sm text-secondary hover:text-primary block transition-colors"
                            >
                              {child.name}
                            </Link>
                          </li>
                        ))}
                      </ul>
                    </div>
                  ))
                )}
              </div>
            )}
          </div>

          <div className="flex items-center gap-6 flex-1">
            {[
              { path: '/', label: 'common.home' },
              { path: '/products', label: 'common.products' },
              { path: '/promotions', label: 'header.promotions' },
              { path: '/news', label: 'header.news' },
              { path: '/contact', label: 'header.contact' },
            ].map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className={cn(
                  'py-3 font-medium transition-colors',
                  (location.pathname === link.path || (link.path !== '/' && location.pathname.startsWith(link.path)))
                    ? 'text-primary'
                    : 'text-secondary hover:text-primary'
                )}
              >
                {t(link.label)}
              </Link>
            ))}
          </div>
        </div>
      </nav>

      {/* Mobile Menu */}
      <AnimatePresence>
        {mobileMenuOpen && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-[1001]"
              onClick={() => setMobileMenuOpen(false)}
            />
            <motion.div
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              className="fixed top-0 right-0 bottom-0 w-80 max-w-full bg-primary z-[1002] overflow-y-auto"
            >
              <div className="flex items-center justify-between p-4 border-b">
                <span className="font-bold text-lg">Menu</span>
                <Button variant="ghost" size="sm" onClick={() => setMobileMenuOpen(false)}>
                  <X size={24} />
                </Button>
              </div>
              <div className="p-4 space-y-4">
                <form onSubmit={handleSearch} className="flex gap-2">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder={t('header.searchPlaceholder')}
                    className="flex-1 px-4 py-2 border rounded-lg text-sm bg-transparent"
                  />
                  <Button type="submit" size="sm"><Search size={18} /></Button>
                </form>
                <nav className="space-y-1">
                  {[
                    { path: '/', label: 'common.home' },
                    { path: '/products', label: 'common.products' },
                    { path: '/promotions', label: 'header.promotions' },
                    { path: '/wishlist', label: 'common.wishlist' },
                    { path: '/account', label: 'common.account.title' },
                  ].map((link) => (
                    <Link
                      key={link.path}
                      to={link.path}
                      onClick={() => setMobileMenuOpen(false)}
                      className="block px-4 py-3 font-medium hover:bg-hover rounded-lg"
                    >
                      {t(link.label)}
                    </Link>
                  ))}
                </nav>
                <div className="flex gap-2 pt-4 border-t">
                  {isAuthenticated ? (
                    <Button fullWidth variant="outline" className="text-red-500 hover:bg-red-50" onClick={handleLogoutClick}>
                      {t('common.logout')}
                    </Button>
                  ) : (
                    <>
                      <Button fullWidth onClick={() => { setMobileMenuOpen(false); navigate('/login'); }}>{t('common.login')}</Button>
                      <Button fullWidth variant="outline" onClick={() => { setMobileMenuOpen(false); navigate('/register'); }}>{t('common.register')}</Button>
                    </>
                  )}
                </div>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>

      <ConfirmDialog
        isOpen={showLogoutConfirm}
        onClose={() => setShowLogoutConfirm(false)}
        onConfirm={confirmLogout}
        title={t('common.logout')}
        message={t('common.messages.logoutConfirmMessage')}
        confirmText={t('common.logout')}
        cancelText={t('common.cancel')}
      />
    </>
  );
});

Header.displayName = 'Header';
export default Header;
