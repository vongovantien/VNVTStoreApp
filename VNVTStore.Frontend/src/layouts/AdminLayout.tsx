import { useState, useRef, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { NavLink, Outlet, useNavigate, useLocation, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  FileText,
  Settings,
  ChevronLeft,
  ChevronRight,
  LogOut,
  Search,
  Sun,
  Moon,
  Menu,
  Globe,
  User as UserIcon,
  HelpCircle,
  FileKey,
  Folder,
  Building2,
  ExternalLink,
  AlertTriangle,
  Command,
  Home,
  Tag,
  Ruler,
  Star,
  Ticket,
  Shield,

  ChevronRight as BreadcrumbSeparator,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button, ConfirmDialog } from '@/components/ui';
import { ToastContainer } from '@/components/ui/Toast';
import { useUIStore, useAuthStore, useToastStore, useNotificationStore } from '@/store';
import { NotificationDropdown, UserMenu } from '@/components/common';
import { useSignalR } from '@/hooks/useSignalR';

// Navigation items
interface NavItem {
  path: string;
  icon: React.ElementType;
  label: string;
  code: string;
  end?: boolean;
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

const navGroups: NavGroup[] = [
  {
    title: 'admin.sidebar.core',
    items: [
      { path: '/admin', icon: LayoutDashboard, label: 'admin.sidebar.dashboard', code: 'DASHBOARD', end: true },
      { path: '/admin/orders', icon: ShoppingCart, label: 'admin.sidebar.orders', code: 'ORDERS', end: false },
      { path: '/admin/customers', icon: Users, label: 'admin.sidebar.customers', code: 'CUSTOMERS', end: false },
    ]
  },
  {
    title: 'admin.sidebar.inventory',
    items: [
      { path: '/admin/categories', icon: Folder, label: 'admin.sidebar.categories', code: 'CATEGORIES', end: false },
      { path: '/admin/products', icon: Package, label: 'admin.sidebar.products', code: 'PRODUCTS', end: false },
      { path: '/admin/suppliers', icon: Building2, label: 'admin.sidebar.suppliers', code: 'SUPPLIERS', end: false },
      { path: '/admin/brands', icon: Tag, label: 'admin.sidebar.brands', code: 'BRANDS', end: false },
      { path: '/admin/units', icon: Ruler, label: 'admin.sidebar.units', code: 'UNITS', end: false },
    ]
  },
  {
    title: 'admin.sidebar.marketing',
    items: [
      { path: '/admin/quotes', icon: FileText, label: 'admin.sidebar.quotes', code: 'QUOTES', end: false },
      { path: '/admin/promotions', icon: Package, label: 'admin.sidebar.promotions', code: 'PROMOTIONS', end: false },
      { path: '/admin/coupons', icon: Ticket, label: 'admin.sidebar.coupons', code: 'COUPONS', end: false },
      { path: '/admin/banners', icon: LayoutDashboard, label: 'admin.sidebar.banners', code: 'BANNERS', end: false },
      { path: '/admin/news', icon: FileText, label: 'admin.sidebar.news', code: 'NEWS', end: false },
      { path: '/admin/reviews', icon: Star, label: 'admin.sidebar.reviews', code: 'REVIEWS', end: false },
    ]
  },
  {
    title: 'admin.sidebar.system',
    items: [
      { path: '/admin/settings', icon: Settings, label: 'admin.sidebar.settings', code: 'SETTINGS', end: false },
      { path: '/admin/audit-logs', icon: FileText, label: 'admin.sidebar.auditLogs', code: 'AUDIT_LOGS', end: false },
      { path: '/admin/roles', icon: Shield, label: 'admin.sidebar.roles', code: 'ROLES', end: false },
    ]
  }
];

export const AdminLayout = () => {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showLangMenu, setShowLangMenu] = useState(false);
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);
  const [showSearchModal, setShowSearchModal] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const { theme, toggleTheme } = useUIStore();
  const { logout, user, isAuthenticated, hasMenu } = useAuthStore();
  const location = useLocation();

  // Filter navigation items based on user's menus
  const filteredNavGroups = useMemo(() => {
    return navGroups
      .map(group => ({
        ...group,
        items: group.items.filter(item => hasMenu(item.code))
      }))
      .filter(group => group.items.length > 0);
  }, [hasMenu]);

  // Refs for dropdowns
  const langBtnRef = useRef<HTMLButtonElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [langMenuPosition, setLangMenuPosition] = useState({ top: 0, left: 0 });

  useEffect(() => {
    if (showLangMenu && langBtnRef.current) {
      const rect = langBtnRef.current.getBoundingClientRect();
      setLangMenuPosition({ top: rect.bottom + 8, left: rect.right - 140 });
    }
  }, [showLangMenu]);


  // SignalR Integration
  const { on, isConnected } = useSignalR();
  const { info } = useToastStore();
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    // Listen for new orders
    const cleanupOrder = on('ReceiveOrderNotification', (data: unknown) => {
       const message = typeof data === 'string' ? data : (data as { Message: string })?.Message || t('admin.notifications.newOrder');
       // Show toast
       info(message);
       // Add to notification list
       addNotification(message);
       
       // Optional: Play a sound
       const audio = new Audio('/notification.mp3'); // Ensure this file exists or remove
       audio.play().catch(e => console.log('Audio play failed', e)); 
    });

    return () => {
        cleanupOrder();
    };
  }, [on, info, addNotification, t]);

  // Ctrl+K keyboard shortcut
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        setShowSearchModal(true);
      }
      if (e.key === 'Escape') {
        setShowSearchModal(false);
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Focus search input when modal opens
  useEffect(() => {
    if (showSearchModal && searchInputRef.current) {
      searchInputRef.current.focus();
    }
  }, [showSearchModal]);

  // Generate breadcrumbs from current path
  const getBreadcrumbs = () => {
    const pathParts = location.pathname.split('/').filter(Boolean);
    const crumbs: { label: string; path: string }[] = [];
    
    let currentPath = '';
    for (const part of pathParts) {
      currentPath += `/${part}`;
      let label = part.charAt(0).toUpperCase() + part.slice(1);
      
      if (part === 'admin') {
          label = t('admin.sidebar.dashboard');
      } else {
          // Try to find localized label
          const translated = t(`admin.sidebar.${part}`);
          if (translated !== `admin.sidebar.${part}`) {
              label = translated;
          } else {
              const commonModule = t(`common.modules.${part}`);
              if (commonModule !== `common.modules.${part}`) {
                  label = commonModule;
              }
          }
      }
      crumbs.push({ label, path: currentPath });
    }
    return crumbs;
  };

  const breadcrumbs = getBreadcrumbs();

  // Click outside handler for language menu
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node;

      if (showLangMenu &&
        langBtnRef.current && !langBtnRef.current.contains(target)) {
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


  if (!isAuthenticated || !user) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  // Case-insensitive check
  if (String(user.role).toLowerCase() !== 'admin') {
      return (
        <div className="flex flex-col items-center justify-center h-screen bg-gray-50 text-gray-800">
            <h1 className="text-2xl font-bold mb-2">403 - {t('common.forbidden', 'Truy cập bị từ chối')}</h1>
            <p className="mb-4">{t('messages.adminOnly', 'Bạn không có quyền truy cập trang quản trị.')}</p>
            <Button onClick={() => navigate('/')}>{t('common.backToHome', 'Về trang chủ')}</Button>
        </div>
      );
  }

  const handleLogout = () => {
    setShowLogoutConfirm(true);
  };

  const confirmLogout = () => {
    setShowLogoutConfirm(false);
    logout();
    navigate('/login');
  };

  const changeLanguage = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem('language', lang);
    setShowLangMenu(false);
  };

  return (
    <div className="flex min-h-screen bg-secondary">
      {/* Sidebar */}
      <aside
        className={cn(
          'fixed left-0 top-0 z-40 h-screen bg-gray-900 text-white transition-all duration-300 flex flex-col',
          sidebarCollapsed ? 'w-20' : 'w-64',
          'hidden lg:block'
        )}
      >
        {/* Logo */}
        <div className="flex items-center justify-between h-16 px-4 border-b border-gray-800">
          <AnimatePresence mode="wait">
            {!sidebarCollapsed && (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="flex items-center gap-2"
              >
                <span className="text-2xl">🏠</span>
                <span className="font-bold text-lg">VNVT Admin</span>
              </motion.div>
            )}
          </AnimatePresence>
          <button
            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
            className="p-2 rounded-lg hover:bg-gray-800 transition-colors"
          >
            {sidebarCollapsed ? <ChevronRight size={20} /> : <ChevronLeft size={20} />}
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 min-h-0 overflow-y-auto custom-scrollbar-dark p-4 space-y-6">
          {filteredNavGroups.map((group, index) => (
            <div key={index}>
              {!sidebarCollapsed && (
                <div className="px-4 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {t(group.title)}
                </div>
              )}
              <div className="space-y-1">
                {group.items.map((item) => (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    end={!!item.end}
                    className={({ isActive }) =>
                      cn(
                        'flex items-center gap-3 px-4 py-2.5 rounded-lg transition-all',
                        isActive
                          ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/25'
                          : 'text-gray-400 hover:text-white hover:bg-gray-800'
                      )
                    }
                  >
                    <item.icon size={20} />
                    <motion.span
                      initial={false}
                      animate={{
                        width: sidebarCollapsed ? 0 : 'auto',
                        opacity: sidebarCollapsed ? 0 : 1,
                      }}
                      transition={{ duration: 0.3 }}
                      className="whitespace-nowrap overflow-hidden"
                    >
                      {t(item.label)}
                    </motion.span>
                  </NavLink>
                ))}
              </div>
            </div>
          ))}
        </nav>

        {/* Logout */}
        <div className="p-4 border-t border-gray-800 shrink-0">
          <button
            onClick={handleLogout}
            className={cn(
              'flex items-center gap-3 w-full px-4 py-3 text-gray-400 rounded-lg hover:text-white hover:bg-gray-800 transition-all'
            )}
          >
            <LogOut size={20} />
            {!sidebarCollapsed && <span>{t('common.logout')}</span>}
          </button>
        </div>
      </aside>

      {/* Mobile Sidebar Overlay */}
      <AnimatePresence>
        {mobileMenuOpen && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setMobileMenuOpen(false)}
            />
            <motion.aside
              initial={{ x: -280 }}
              animate={{ x: 0 }}
              exit={{ x: -280 }}
              className="fixed left-0 top-0 z-50 h-screen w-64 bg-gray-900 text-white lg:hidden flex flex-col"
            >
              {/* Same content as desktop sidebar */}
              <div className="flex items-center h-16 px-4 border-b border-gray-800 shrink-0">
                <span className="text-2xl">🏠</span>
                <span className="font-bold text-lg ml-2">VNVT Admin</span>
                <button
                  onClick={() => setMobileMenuOpen(false)}
                  className="ml-auto p-2 text-gray-400 hover:text-white"
                >
                  <ChevronLeft size={20} />
                </button>
              </div>
              <nav className="flex-1 min-h-0 overflow-y-auto custom-scrollbar-dark p-4 space-y-6">
                {filteredNavGroups.map((group, index) => (
                  <div key={index}>
                    <div className="px-4 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                      {t(group.title)}
                    </div>
                    <div className="space-y-1">
                      {group.items.map((item) => (
                        <NavLink
                          key={item.path}
                          to={item.path}
                          end={!!item.end}
                          onClick={() => setMobileMenuOpen(false)}
                          className={({ isActive }) =>
                            cn(
                              'flex items-center gap-3 px-4 py-3 rounded-lg transition-all',
                              isActive
                                ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/25'
                                : 'text-gray-400 hover:text-white hover:bg-gray-800'
                            )
                          }
                        >
                          <item.icon size={20} />
                          <span>{t(item.label)}</span>
                        </NavLink>
                      ))}
                    </div>
                  </div>
                ))}
              </nav>
              
              {/* Logout button for mobile */}
              <div className="p-4 border-t border-gray-800 shrink-0">
                <button
                  onClick={handleLogout}
                  className="flex items-center gap-3 w-full px-4 py-3 text-gray-400 rounded-lg hover:text-white hover:bg-gray-800 transition-all"
                >
                  <LogOut size={20} />
                  <span>{t('common.logout')}</span>
                </button>
              </div>
            </motion.aside>
          </>
        )}
      </AnimatePresence>

      {/* Main Content */}
      <div
        className={cn(
          'flex-1 transition-all duration-300',
          sidebarCollapsed ? 'lg:ml-20' : 'lg:ml-64'
        )}
      >
        {/* Top bar */}
        <header className="sticky top-0 z-30 bg-primary shadow-sm">
          <div className="flex items-center justify-between h-16 px-4 lg:px-6">
            {/* Mobile menu button */}
            <button
              className="p-2 rounded-lg hover:bg-hover lg:hidden"
              onClick={() => setMobileMenuOpen(true)}
            >
              <Menu size={24} />
            </button>

            {/* Breadcrumbs - Hidden on mobile */}
            <div className="hidden lg:flex items-center gap-1 text-sm">
              <Home size={14} className="text-tertiary" />
              {breadcrumbs.map((crumb, index) => (
                <div key={crumb.path} className="flex items-center gap-1">
                  <BreadcrumbSeparator size={14} className="text-tertiary" />
                  {index === breadcrumbs.length - 1 ? (
                    <span className="font-medium text-primary">{crumb.label}</span>
                  ) : (
                    <NavLink to={crumb.path} className="text-secondary hover:text-primary transition-colors">
                      {crumb.label}
                    </NavLink>
                  )}
                </div>
              ))}
            </div>

            {/* Global Search with Ctrl+K */}
            <div className="hidden md:block flex-1 max-w-sm mx-4">
              <button
                onClick={() => setShowSearchModal(true)}
                className="w-full flex items-center gap-3 px-4 py-2 bg-secondary rounded-lg border border-transparent hover:border-indigo-500 transition-all group"
              >
                <Search size={18} className="text-tertiary group-hover:text-indigo-500" />
                <span className="flex-1 text-left text-sm text-tertiary">{t('common.search', 'Tìm kiếm...')}</span>
                <kbd className="hidden sm:inline-flex items-center gap-1 px-2 py-0.5 text-[10px] font-medium text-tertiary bg-primary rounded border">
                  <Command size={10} /> K
                </kbd>
              </button>
            </div>

            {/* Quick Stats */}
            <div className="hidden lg:flex items-center gap-2 mr-2">
              <button 
                onClick={() => navigate('/admin/orders?status=pending')}
                className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs font-medium bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-400 rounded-full hover:bg-orange-200 dark:hover:bg-orange-900/50 transition-colors"
                title={t('admin.tooltips.pendingOrders')}
              >
                <ShoppingCart size={14} />
                <span>5</span>
              </button>
              <button 
                onClick={() => navigate('/admin/products?stock=0')}
                className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs font-medium bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400 rounded-full hover:bg-red-200 dark:hover:bg-red-900/50 transition-colors"
                title={t('admin.tooltips.outOfStock')}
              >
                <AlertTriangle size={14} />
                <span>2</span>
              </button>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
              {/* Language Switcher */}
              <div className="relative">
                <Button ref={langBtnRef} variant="ghost" size="sm" onClick={() => setShowLangMenu(!showLangMenu)}>
                  <Globe size={20} />
                  <span className="text-xs ml-1 uppercase">{i18n.language}</span>
                </Button>
                {showLangMenu && createPortal(
                  <div
                    className="fixed bg-primary rounded-lg shadow-xl border border-border p-1 min-w-[140px] z-[9999]"
                    style={{
                      top: langMenuPosition.top,
                      left: langMenuPosition.left,
                    }}
                  >
                    <button
                      onClick={() => { changeLanguage('vi'); setShowLangMenu(false); }}
                      className={cn(
                        'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                        i18n.language === 'vi'
                          ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-600 dark:text-indigo-400'
                          : 'hover:bg-hover text-primary'
                      )}
                    >
                      🇻🇳 Tiếng Việt
                    </button>
                    <button
                      onClick={() => { changeLanguage('en'); setShowLangMenu(false); }}
                      className={cn(
                        'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                        i18n.language === 'en'
                          ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-600 dark:text-indigo-400'
                          : 'hover:bg-hover text-primary'
                      )}
                    >
                      🇺🇸 English
                    </button>
                  </div>,
                  document.body
                )}
              </div>

              <Button variant="ghost" size="sm" onClick={toggleTheme}>
                {theme === 'light' ? <Moon size={20} /> : <Sun size={20} />}
              </Button>

              {/* Notifications Dropdown */}
              <NotificationDropdown 
                isConnected={isConnected}
                onNotificationClick={() => navigate('/admin/orders')}
              />

              {/* View Store */}
              <a 
                href="/" 
                className="hidden lg:flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 rounded-lg hover:bg-indigo-200 dark:hover:bg-indigo-900/50 transition-colors"
              >
                <ExternalLink size={14} />
                {t('admin.viewStore', 'Xem cửa hàng')}
              </a>

              {/* User menu */}
              <UserMenu 
                onLogout={handleLogout}
                items={[
                  {
                    label: t('admin.userMenu.accountSettings'),
                    icon: UserIcon,
                    link: '/admin/settings'
                  },
                  {
                    label: t('admin.userMenu.support'),
                    icon: HelpCircle
                  },
                  {
                    label: t('admin.userMenu.license'),
                    icon: FileKey
                  }
                ]}
              />
            </div>
          </div>
        </header>

        {/* Page content */}
        <main className="p-4 lg:p-6">
          <Outlet />
        </main>
      </div>
      <ToastContainer />
      <ConfirmDialog
        isOpen={showLogoutConfirm}
        onClose={() => setShowLogoutConfirm(false)}
        onConfirm={confirmLogout}
        title={t('messages.logoutConfirmTitle')}
        message={t('messages.logoutConfirmMessage')}
        confirmText={t('common.logout')}
        variant="danger"
      />

      {/* Global Search Modal */}
      <AnimatePresence>
        {showSearchModal && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-[999]"
              onClick={() => setShowSearchModal(false)}
            />
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: -20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: -20 }}
              className="fixed top-[15%] left-1/2 -translate-x-1/2 w-full max-w-lg bg-primary rounded-2xl shadow-2xl border border-border z-[1000] overflow-hidden"
            >
              {/* Search Header with Gradient */}
              <div className="bg-gradient-to-r from-indigo-500 to-purple-500 p-4">
                <div className="flex items-center gap-3 bg-white/10 backdrop-blur rounded-xl px-4 py-3">
                  <Search size={20} className="text-white/80" />
                  <input
                    ref={searchInputRef}
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder={t('admin.searchPlaceholder', 'Tìm đơn hàng, sản phẩm, khách hàng...')}
                    className="flex-1 bg-transparent outline-none text-white placeholder:text-white/60 text-sm font-medium"
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' && searchQuery.trim()) {
                        setShowSearchModal(false);
                        navigate(`/admin/products?search=${encodeURIComponent(searchQuery)}`);
                      }
                    }}
                  />
                  <kbd className="px-2 py-1 text-[10px] font-bold text-white/80 bg-white/20 rounded-lg">
                    ESC
                  </kbd>
                </div>
              </div>
              
              {/* Quick Links */}
              <div className="p-4">
                <p className="mb-3 text-xs font-semibold text-tertiary uppercase tracking-widest">
                  ⚡ {t('common.quickAccess', 'Truy cập nhanh')}
                </p>
                <div className="grid grid-cols-3 gap-2">
                  <button 
                    onClick={() => { setShowSearchModal(false); navigate('/admin/orders'); }}
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-orange-50 dark:bg-orange-900/20 hover:bg-orange-100 dark:hover:bg-orange-900/30 transition-colors group"
                  >
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-orange-400 to-orange-600 flex items-center justify-center shadow-lg shadow-orange-500/30">
                      <ShoppingCart size={18} className="text-white" />
                    </div>
                    <span className="text-xs font-medium text-secondary group-hover:text-primary">{t('admin.sidebar.orders')}</span>
                  </button>
                  <button 
                    onClick={() => { setShowSearchModal(false); navigate('/admin/products'); }}
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-blue-50 dark:bg-blue-900/20 hover:bg-blue-100 dark:hover:bg-blue-900/30 transition-colors group"
                  >
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center shadow-lg shadow-blue-500/30">
                      <Package size={18} className="text-white" />
                    </div>
                    <span className="text-xs font-medium text-secondary group-hover:text-primary">{t('admin.sidebar.products')}</span>
                  </button>
                  <button 
                    onClick={() => { setShowSearchModal(false); navigate('/admin/customers'); }}
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-emerald-50 dark:bg-emerald-900/20 hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors group"
                  >
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-emerald-600 flex items-center justify-center shadow-lg shadow-emerald-500/30">
                      <Users size={18} className="text-white" />
                    </div>
                    <span className="text-xs font-medium text-secondary group-hover:text-primary">{t('admin.sidebar.customers')}</span>
                  </button>
                </div>
              </div>
              
              {/* Footer hint */}
              <div className="px-4 py-3 bg-secondary border-t border-border text-xs text-tertiary flex items-center justify-between">
                <span>{t('admin.search.pressEnter')}</span>
                <span className="flex items-center gap-1">
                  <Command size={10} /> + K {t('admin.search.ctrlKHint')}
                </span>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </div>
  );
};

export default AdminLayout;
