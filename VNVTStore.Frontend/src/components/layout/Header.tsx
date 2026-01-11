import { memo, useState, useRef, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { Link, useLocation } from 'react-router-dom';
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
  Globe,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';
import { useCartStore, useWishlistStore, useUIStore, useCompareStore } from '@/store';
import { useClickOutside } from '@/hooks';
import { useCategories } from '@/hooks/useProducts';

export const Header = memo(() => {
  const { t, i18n } = useTranslation();
  const location = useLocation();
  const { data: categories = [] } = useCategories(); // Fetch categories
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showCategories, setShowCategories] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showLangMenu, setShowLangMenu] = useState(false);


  const categoriesRef = useClickOutside<HTMLDivElement>(() => setShowCategories(false));
  const userMenuRef = useClickOutside<HTMLDivElement>(() => setShowUserMenu(false));
  // const langMenuRef = useClickOutside<HTMLDivElement>(() => setShowLangMenu(false)); // Temporarily removed
  const langMenuRef = useRef<HTMLDivElement>(null); // Use simple ref for button
  const langDropdownRef = useRef<HTMLDivElement>(null); // Ref for dropdown portal

  const cartCount = useCartStore((state) => state.getItemCount());
  const wishlistCount = useWishlistStore((state) => state.items.length);
  const compareCount = useCompareStore((state) => state.items.length);
  const { theme, toggleTheme, setCartOpen } = useUIStore();

  // Manual click outside handler for language menu
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node;
      // Check if click is outside both the button and the dropdown
      const isOutsideButton = langMenuRef.current && !langMenuRef.current.contains(target);
      const isOutsideDropdown = langDropdownRef.current && !langDropdownRef.current.contains(target);

      if (isOutsideButton && isOutsideDropdown) {
        setShowLangMenu(false);
      }
    };
    if (showLangMenu) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showLangMenu]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      window.location.href = `/products?search=${encodeURIComponent(searchQuery)}`;
    }
  };

  const changeLanguage = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem('language', lang);
    setShowLangMenu(false);
  };

  return (
    <header className="sticky top-0 z-[100] bg-primary shadow-sm border-b transition-colors duration-200">
      {/* Top Bar */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-slate-300 text-xs py-2">
        <div className="container mx-auto px-4 flex justify-between items-center">
          <div className="flex gap-6">
            <a href="tel:1900123456" className="flex items-center gap-1 hover:text-white transition-colors">
              <Phone size={12} />
              <span>{t('header.hotline')}</span>
            </a>
            <a href="https://zalo.me" className="flex items-center gap-1 text-blue-400 hover:text-blue-300 transition-colors">
              <MessageCircle size={12} />
              <span>{t('header.chatZalo')}</span>
            </a>
          </div>
          <div className="hidden md:flex gap-4">
            <Link to="/tracking" className="hover:text-white transition-colors">{t('header.trackOrder')}</Link>
            <Link to="/support" className="hover:text-white transition-colors">{t('header.support')}</Link>
            <Link to="/about" className="hover:text-white transition-colors">{t('header.about')}</Link>
          </div>
        </div>
      </div>

      {/* Main Header */}
      <div className="border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-6">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 flex-shrink-0">
            <span className="text-3xl">🏠</span>
            <span className="flex flex-col leading-tight">
              <span className="text-xl font-extrabold bg-gradient-to-r from-primary to-purple-500 bg-clip-text text-transparent">
                VNVT
              </span>
              <span className="text-xs text-secondary font-medium">Store</span>
            </span>
          </Link>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="flex-1 max-w-xl mx-8 hidden md:block">
            <div className="flex items-center bg-tertiary border border-transparent focus-within:border-indigo-500 rounded-full pl-4 pr-1 py-1 transition-all duration-200">
              <Search size={20} className="text-secondary" />
              <div className="flex-1 ml-3 mr-2">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder={t('header.searchPlaceholder')}
                  className="w-full !bg-transparent text-sm outline-none text-primary placeholder:text-tertiary !border-0 !ring-0 !shadow-none focus:ring-0"
                />
              </div>
              <Button type="submit" rounded size="sm" className="px-6 h-9 shrink-0">
                {t('common.search')}
              </Button>
            </div>
          </form>

          {/* Actions */}
          <div className="flex items-center gap-1">
            {/* Language Switcher */}
            {/* Language Switcher */}
            <div className="relative" ref={langMenuRef}>
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  setShowLangMenu(!showLangMenu);
                }}
              >
                <Globe size={20} />
                <span className="text-xs ml-1 uppercase">{(i18n.language || 'vi').substring(0, 2)}</span>
              </Button>
            </div>
            {showLangMenu && createPortal(
              <div
                ref={langDropdownRef}
                className="fixed bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-slate-200 dark:border-slate-700 p-1 min-w-[140px] z-[9999]"
                style={{
                  top: langMenuRef.current ? langMenuRef.current.getBoundingClientRect().bottom + 8 : 0,
                  left: langMenuRef.current ? langMenuRef.current.getBoundingClientRect().left : 0,
                }}
              >
                <button
                  onClick={() => changeLanguage('vi')}
                  className={cn(
                    'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                    (i18n.language || 'vi').startsWith('vi')
                      ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-600 dark:text-indigo-400'
                      : 'hover:bg-gray-100 dark:hover:bg-slate-700 text-gray-700 dark:text-gray-300'
                  )}
                >
                  🇻🇳 Tiếng Việt
                </button>
                <button
                  onClick={() => changeLanguage('en')}
                  className={cn(
                    'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                    (i18n.language || 'en').startsWith('en')
                      ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-600 dark:text-indigo-400'
                      : 'hover:bg-gray-100 dark:hover:bg-slate-700 text-gray-700 dark:text-gray-300'
                  )}
                >
                  🇺🇸 English
                </button>
              </div>,
              document.body
            )}

            {/* Theme Toggle */}
            <Button variant="ghost" size="sm" onClick={toggleTheme}>
              {theme === 'light' ? <Moon size={20} /> : <Sun size={20} />}
            </Button>

            {/* Compare */}
            <Link to="/compare" className="hidden sm:flex">
              <Button variant="ghost" size="sm" className="relative">
                <Scale size={20} />
                {compareCount > 0 && (
                  <span className="absolute -top-1 -right-1 w-4 h-4 bg-secondary text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                    {compareCount}
                  </span>
                )}
              </Button>
            </Link>

            {/* Wishlist */}
            <Link to="/wishlist" className="hidden sm:flex">
              <Button variant="ghost" size="sm" className="relative">
                <Heart size={20} />
                {wishlistCount > 0 && (
                  <span className="absolute -top-1 -right-1 w-4 h-4 bg-secondary text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                    {wishlistCount}
                  </span>
                )}
              </Button>
            </Link>

            {/* Cart */}
            <Button variant="ghost" size="sm" className="relative" onClick={() => setCartOpen(true)}>
              <ShoppingCart size={20} />
              {cartCount > 0 && (
                <span className="absolute -top-1 -right-1 w-5 h-5 bg-error text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                  {cartCount}
                </span>
              )}
            </Button>

            {/* User Menu */}
            <div className="relative hidden lg:block" ref={userMenuRef}>
              <Button variant="ghost" size="sm" onClick={() => setShowUserMenu(!showUserMenu)}>
                <User size={20} />
                <ChevronDown size={14} />
              </Button>
              <AnimatePresence>
                {showUserMenu && (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    className="absolute top-full right-0 mt-2 bg-primary rounded-lg shadow-xl border p-2 min-w-[180px] z-50"
                  >
                    <Link to="/login" className="block px-4 py-2 text-sm hover:bg-secondary rounded-md transition-colors">
                      {t('common.login')}
                    </Link>
                    <Link to="/register" className="block px-4 py-2 text-sm hover:bg-secondary rounded-md transition-colors">
                      {t('common.register')}
                    </Link>
                    <hr className="my-2" />
                    <Link to="/account" className="block px-4 py-2 text-sm hover:bg-secondary rounded-md transition-colors">
                      {t('common.account')}
                    </Link>
                    <Link to="/account/orders" className="block px-4 py-2 text-sm hover:bg-secondary rounded-md transition-colors">
                      {t('account.orders')}
                    </Link>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            {/* Mobile Menu Button */}
            <Button variant="ghost" size="sm" className="lg:hidden" onClick={() => setMobileMenuOpen(true)}>
              <Menu size={24} />
            </Button>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="hidden lg:block border-b bg-primary transition-colors duration-200">
        <div className="container mx-auto px-4 flex items-center gap-6">
          {/* Categories Dropdown */}
          <div className="relative" ref={categoriesRef}>
            <Button
              onClick={() => setShowCategories(!showCategories)}
              className="rounded-none py-3"
              leftIcon={<Menu size={18} />}
              rightIcon={<ChevronDown size={16} />}
            >
              {t('header.categoryMenu')}
            </Button>
            <AnimatePresence>
              {showCategories && (
                <div className="absolute top-full left-0 bg-primary rounded-b-xl shadow-xl border-t-0 p-4 min-w-[800px] z-50 grid grid-cols-3 gap-6">
                  {categories.filter(c => !c.parentCode).map((parent) => (
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
                  ))}
                </div>
              )}
            </AnimatePresence>
          </div>

          {/* Nav Links */}
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
                  (location.pathname === link.path ||
                    (link.path !== '/' && location.pathname.startsWith(link.path)))
                    ? 'text-primary'
                    : 'text-secondary hover:text-primary'
                )}
              >
                {t(link.label)}
              </Link>
            ))}
          </div>

          {/* Promo */}
          <div className="flex items-center gap-2 text-sm">
            <span className="bg-secondary text-white px-2 py-0.5 rounded-full text-xs font-bold">🔥 HOT</span>
            <span className="text-secondary">{t('header.freeShipping')}</span>
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
              className="fixed inset-0 bg-black/50 z-50"
              onClick={() => setMobileMenuOpen(false)}
            />
            <motion.div
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'tween' }}
              className="fixed top-0 right-0 bottom-0 w-80 max-w-full bg-primary z-50 overflow-y-auto"
            >
              <div className="flex items-center justify-between p-4 border-b">
                <span className="font-bold text-lg">Menu</span>
                <Button variant="ghost" size="sm" onClick={() => setMobileMenuOpen(false)}>
                  <X size={24} />
                </Button>
              </div>

              <div className="p-4 space-y-4">
                {/* Mobile Search */}
                <form onSubmit={handleSearch} className="flex gap-2">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder={t('header.searchPlaceholder')}
                    className="flex-1 px-4 py-2 border rounded-lg text-sm"
                  />
                  <Button type="submit" size="sm">
                    <Search size={18} />
                  </Button>
                </form>

                {/* Mobile Nav */}
                <nav className="space-y-1">
                  {[
                    { path: '/', label: 'common.home' },
                    { path: '/products', label: 'common.products' },
                    { path: '/promotions', label: 'header.promotions' },
                    { path: '/wishlist', label: 'common.wishlist' },
                    { path: '/compare', label: 'common.compare' },
                    { path: '/account', label: 'common.account' },
                  ].map((link) => (
                    <Link
                      key={link.path}
                      to={link.path}
                      onClick={() => setMobileMenuOpen(false)}
                      className="block px-4 py-3 font-medium hover:bg-secondary rounded-lg transition-colors"
                    >
                      {t(link.label)}
                    </Link>
                  ))}
                </nav>

                {/* Categories */}
                <div>
                  <h3 className="text-xs font-semibold text-tertiary uppercase mb-2 px-4">
                    {t('common.categories')}
                  </h3>
                  {categories.map((category) => (
                    <Link
                      key={category.code}
                      to={`/products?category=${category.code}`}
                      className="block px-4 py-2 hover:bg-secondary/50 transition-colors rounded-lg"
                      onClick={() => setShowCategories(false)}
                    >
                      {category.name}
                    </Link>
                  ))}
                </div>

                {/* Auth buttons */}
                <div className="flex gap-2 pt-4 border-t">
                  <Button fullWidth onClick={() => { setMobileMenuOpen(false); window.location.href = '/login'; }}>
                    {t('common.login')}
                  </Button>
                  <Button fullWidth variant="outline" onClick={() => { setMobileMenuOpen(false); window.location.href = '/register'; }}>
                    {t('common.register')}
                  </Button>
                </div>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </header>
  );
});

Header.displayName = 'Header';

export default Header;
