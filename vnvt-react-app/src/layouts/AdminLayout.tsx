import { useState } from 'react';
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
const navItems = [
  { path: '/admin', icon: LayoutDashboard, label: 'admin.dashboard', end: true },
  { path: '/admin/products', icon: Package, label: 'admin.products' },
  { path: '/admin/categories', icon: Folder, label: 'admin.categories' },
  { path: '/admin/suppliers', icon: Building2, label: 'admin.suppliers' },
  { path: '/admin/orders', icon: ShoppingCart, label: 'admin.orders' },
  { path: '/admin/customers', icon: Users, label: 'admin.customers' },
  { path: '/admin/quotes', icon: FileText, label: 'admin.quotes' },
  { path: '/admin/settings', icon: Settings, label: 'admin.settings' },
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
        <nav className="p-4 space-y-2">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.end}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 px-4 py-3 rounded-lg transition-all',
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
              <nav className="p-4 space-y-2">
                {navItems.map((item) => (
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
                <Button variant="ghost" size="sm" onClick={() => setShowLangMenu(!showLangMenu)}>
                  <Globe size={20} />
                  <span className="text-xs ml-1 uppercase">{i18n.language}</span>
                </Button>
                <AnimatePresence>
                  {showLangMenu && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      className="absolute top-full right-0 mt-2 bg-primary rounded-lg shadow-xl border p-1 min-w-[120px] z-50"
                    >
                      <button
                        onClick={() => changeLanguage('vi')}
                        className={cn(
                          'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                          i18n.language === 'vi' ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-secondary'
                        )}
                      >
                        🇻🇳 Tiếng Việt
                      </button>
                      <button
                        onClick={() => changeLanguage('en')}
                        className={cn(
                          'flex items-center gap-2 w-full px-3 py-2 text-sm rounded-md transition-colors',
                          i18n.language === 'en' ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-secondary'
                        )}
                      >
                        🇺🇸 English
                      </button>
                    </motion.div>
                  )}
                </AnimatePresence>
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
                  onClick={() => setShowUserMenu(!showUserMenu)}
                  className="flex items-center gap-3 hover:opacity-80 transition-opacity"
                >
                  <div className="w-8 h-8 rounded-full bg-gradient-to-r from-indigo-500 to-purple-500 flex items-center justify-center text-white font-bold text-sm">
                    {user?.name?.charAt(0) || 'A'}
                  </div>
                  <div className="hidden sm:block text-left">
                    <p className="text-sm font-medium text-primary">{user?.name || 'Admin'}</p>
                    <p className="text-xs text-tertiary">{user?.email || 'admin@example.com'}</p>
                  </div>
                  <ChevronDown size={16} className="hidden sm:block text-tertiary" />
                </button>
                <AnimatePresence>
                  {showUserMenu && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      className="absolute top-full right-0 mt-2 bg-primary rounded-lg shadow-xl border py-2 min-w-[200px] z-50"
                    >
                      <div className="px-4 py-2 border-b">
                        <p className="text-xs text-tertiary">Signed in as</p>
                        <p className="text-sm font-medium text-primary truncate">{user?.email || 'admin@example.com'}</p>
                      </div>
                      <div className="py-1">
                        <button
                          onClick={() => { setShowUserMenu(false); navigate('/admin/settings'); }}
                          className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-secondary transition-colors"
                        >
                          <UserIcon size={16} />
                          Account settings
                        </button>
                        <button
                          className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-secondary transition-colors"
                        >
                          <HelpCircle size={16} />
                          Support
                        </button>
                        <button
                          className="flex items-center gap-3 w-full px-4 py-2 text-sm text-secondary hover:bg-secondary transition-colors"
                        >
                          <FileKey size={16} />
                          License
                        </button>
                      </div>
                      <div className="border-t py-1">
                        <button
                          onClick={handleLogout}
                          className="flex items-center gap-3 w-full px-4 py-2 text-sm text-error hover:bg-error/10 transition-colors"
                        >
                          <LogOut size={16} />
                          Sign out
                        </button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
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
