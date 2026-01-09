import { useState, useRef, useEffect } from 'react';
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
  Bell,
  Search,
  Sun,
  Moon,
  Menu,
  Globe,
  ChevronDown,
  User as UserIcon,
  HelpCircle,
  FileKey,
  Folder,
  Building2,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button, Input, ConfirmDialog } from '@/components/ui';
import { ToastContainer } from '@/components/ui/Toast';
import { useUIStore, useAuthStore } from '@/store';

// Navigation items
const navGroups = [
  {
    title: 'Core',
    items: [
      { path: '/admin', icon: LayoutDashboard, label: 'admin.dashboard', end: true },
      { path: '/admin/orders', icon: ShoppingCart, label: 'admin.orders' },
      { path: '/admin/customers', icon: Users, label: 'admin.customers' },
    ]
  },
  {
    title: 'Inventory',
    items: [
      { path: '/admin/categories', icon: Folder, label: 'admin.categories' },
      { path: '/admin/products', icon: Package, label: 'admin.products' },
      { path: '/admin/suppliers', icon: Building2, label: 'admin.suppliers' },
    ]
  },
  {
    title: 'Marketing',
    items: [
      { path: '/admin/quotes', icon: FileText, label: 'admin.quotes' },
      // Add Coupons/FlashSale when available
    ]
  },
  {
    title: 'System',
    items: [
      { path: '/admin/settings', icon: Settings, label: 'admin.settings' },
    ]
  }
];

export const AdminLayout = () => {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showLangMenu, setShowLangMenu] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);
  const { theme, toggleTheme } = useUIStore();
  const { logout, user, isAuthenticated } = useAuthStore();
  const location = useLocation();

  // Refs for dropdowns
  const langBtnRef = useRef<HTMLButtonElement>(null);
  const langDropdownRef = useRef<HTMLDivElement>(null);
  const userBtnRef = useRef<HTMLButtonElement>(null);
  const userDropdownRef = useRef<HTMLDivElement>(null);

  // Click outside handler
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node;

      // Language Menu
      if (showLangMenu && 
          langBtnRef.current && !langBtnRef.current.contains(target) && 
          langDropdownRef.current && !langDropdownRef.current.contains(target)) {
        setShowLangMenu(false);
      }

      // User Menu
      if (showUserMenu && 
          userBtnRef.current && !userBtnRef.current.contains(target) && 
          userDropdownRef.current && !userDropdownRef.current.contains(target)) {
        setShowUserMenu(false);
      }
    };

    if (showLangMenu || showUserMenu) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showLangMenu, showUserMenu]);

  if (!isAuthenticated || !user || user.role !== 'admin') {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
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
          'fixed left-0 top-0 z-40 h-screen bg-gray-900 text-white transition-all duration-300',
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
        <nav className="p-4 space-y-6">
          {navGroups.map((group, index) => (
            <div key={index}>
              {!sidebarCollapsed && (
                <div className="px-4 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {group.title}
                </div>
              )}
              <div className="space-y-1">
                {group.items.map((item) => (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    end={item.end}
                    className={({ isActive }) =>
                      cn(
                        'flex items-center gap-3 px-4 py-2.5 rounded-lg transition-all',
                        isActive
                          ? 'bg-primary text-white shadow-lg shadow-primary/25'
                          : 'text-gray-400 hover:text-white hover:bg-gray-800'
                      )
                    }
                  >
                    <item.icon size={20} />
                    <AnimatePresence mode="wait">
                      {!sidebarCollapsed && (
                        <motion.span
                          initial={{ opacity: 0, width: 0 }}
                          animate={{ opacity: 1, width: 'auto' }}
                          exit={{ opacity: 0, width: 0 }}
                          className="whitespace-nowrap"
                        >
                          {t(item.label)}
                        </motion.span>
                      )}
                    </AnimatePresence>
                  </NavLink>
                ))}
              </div>
            </div>
          ))}
        </nav>

        {/* Logout */}
        <div className="absolute bottom-4 left-4 right-4">
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
              className="fixed left-0 top-0 z-50 h-screen w-64 bg-gray-900 text-white lg:hidden"
            >
              {/* Same content as desktop sidebar */}
              <div className="flex items-center h-16 px-4 border-b border-gray-800">
                <span className="text-2xl">🏠</span>
                <span className="font-bold text-lg ml-2">VNVT Admin</span>
              </div>
              <nav className="p-4 space-y-6">
                {navGroups.map((group, index) => (
                  <div key={index}>
                    <div className="px-4 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                      {group.title}
                    </div>
                    <div className="space-y-1">
                      {group.items.map((item) => (
                        <NavLink
                          key={item.path}
                          to={item.path}
                          end={item.end}
                          onClick={() => setMobileMenuOpen(false)}
                          className={({ isActive }) =>
                            cn(
                              'flex items-center gap-3 px-4 py-3 rounded-lg transition-all',
                              isActive
                                ? 'bg-primary text-white'
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
              className="p-2 rounded-lg hover:bg-secondary lg:hidden"
              onClick={() => setMobileMenuOpen(true)}
            >
              <Menu size={24} />
            </button>

            {/* Search */}
            <div className="hidden md:block flex-1 max-w-md ml-4">
              <Input
                placeholder="Tìm kiếm..."
                leftIcon={<Search size={18} />}
                size="sm"
              />
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
                    ref={langDropdownRef}
                    className="fixed bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-slate-200 dark:border-slate-700 p-1 min-w-[140px] z-[9999]"
                    style={{
                      top: langBtnRef.current ? langBtnRef.current.getBoundingClientRect().bottom + 8 : 0,
                      left: langBtnRef.current ? langBtnRef.current.getBoundingClientRect().right - 140 : 0,
                    }}
                  >
                    <button
                      onClick={() => changeLanguage('vi')}
                      className={cn(
                        'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                        i18n.language === 'vi' 
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
                        i18n.language === 'en' 
                          ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-600 dark:text-indigo-400' 
                          : 'hover:bg-gray-100 dark:hover:bg-slate-700 text-gray-700 dark:text-gray-300'
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

              <Button variant="ghost" size="sm" className="relative">
                <Bell size={20} />
                <span className="absolute top-1 right-1 w-2 h-2 bg-error rounded-full" />
              </Button>

              {/* User menu */}
              <div className="relative ml-4 pl-4 border-l">
                <button
                  ref={userBtnRef}
                  onClick={() => setShowUserMenu(!showUserMenu)}
                  className="flex items-center gap-3 hover:opacity-80 transition-opacity"
                >
                  <div className="w-8 h-8 rounded-full bg-gradient-to-r from-indigo-500 to-purple-500 flex items-center justify-center text-white font-bold text-sm">
                    {user?.email?.charAt(0).toUpperCase() || 'A'}
                  </div>
                  <div className="hidden sm:block text-left">
                    <p className="text-sm font-medium text-primary">{user?.email || 'Admin'}</p>
                    <p className="text-xs text-tertiary">{user?.email || 'admin@example.com'}</p>
                  </div>
                  <ChevronDown size={16} className="hidden sm:block text-tertiary" />
                </button>
                {showUserMenu && createPortal(
                  <div
                    ref={userDropdownRef}
                    className="fixed bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-slate-200 dark:border-slate-700 py-2 min-w-[200px] z-[9999]"
                    style={{
                      top: userBtnRef.current ? userBtnRef.current.getBoundingClientRect().bottom + 8 : 0,
                      left: userBtnRef.current ? userBtnRef.current.getBoundingClientRect().right - 200 : 0,
                    }}
                  >
                    <div className="px-4 py-2 border-b border-slate-100 dark:border-slate-700">
                      <p className="text-xs text-tertiary">Signed in as</p>
                      <p className="text-sm font-medium text-primary truncate">{user?.email || 'admin@example.com'}</p>
                    </div>
                    <div className="py-1">
                      <button
                        onClick={() => { setShowUserMenu(false); navigate('/admin/settings'); }}
                        className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
                      >
                        <UserIcon size={16} />
                        Account settings
                      </button>
                      <button
                        className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
                      >
                        <HelpCircle size={16} />
                        Support
                      </button>
                      <button
                        className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
                      >
                        <FileKey size={16} />
                        License
                      </button>
                    </div>
                    <div className="border-t border-slate-100 dark:border-slate-700 py-1">
                      <button
                        onClick={handleLogout}
                        className="flex items-center gap-3 w-full px-4 py-2 text-sm text-error hover:bg-red-50 dark:hover:bg-red-900/10 transition-colors"
                      >
                        <LogOut size={16} />
                        Sign out
                      </button>
                    </div>
                  </div>,
                  document.body
                )}
              </div>
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
    </div>
  );
};

export default AdminLayout;
