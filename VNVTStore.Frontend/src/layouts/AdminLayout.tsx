import { useState, useEffect, useMemo } from 'react';
import { Outlet, useNavigate, useLocation, Navigate } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { useTranslation } from 'react-i18next';
import { Button, ConfirmDialog } from '@/components/ui';
import { ToastContainer } from '@/components/ui/Toast';
import { useUIStore, useAuthStore } from '@/store';
import { cn } from '@/utils/cn';

// Extracted Components & Hooks
import { AdminSidebar, navGroups } from './AdminSidebar';
import { AdminHeader } from './AdminHeader';
import { AdminSearchModal } from './AdminSearchModal';
import { useAdminNotifications } from './useAdminNotifications';

export const AdminLayout = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);
  const [showSearchModal, setShowSearchModal] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const { theme, toggleTheme } = useUIStore();
  const { logout, user, isAuthenticated, hasMenu } = useAuthStore();
  const { isConnected } = useAdminNotifications();

  // Filter navigation items based on user's menus
  const filteredNavGroups = useMemo(() => {
    return navGroups
      .map(group => ({
        ...group,
        items: group.items.filter(item => hasMenu(item.code))
      }))
      .filter(group => group.items.length > 0);
  }, [hasMenu]);

  // Keyboard shortcuts
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

  // Generate breadcrumbs
  const breadcrumbs = useMemo(() => {
    const pathParts = location.pathname.split('/').filter(Boolean);
    const crumbs: { label: string; path: string }[] = [];
    let currentPath = '';
    
    for (const part of pathParts) {
      currentPath += `/${part}`;
      let label = part.charAt(0).toUpperCase() + part.slice(1);
      
      if (part === 'admin') {
          label = t('admin.sidebar.dashboard');
      } else {
          const translated = t(`admin.sidebar.${part}`);
          label = translated !== `admin.sidebar.${part}` ? translated : (t(`common.modules.${part}`) !== `common.modules.${part}` ? t(`common.modules.${part}`) : label);
      }
      crumbs.push({ label, path: currentPath });
    }
    return crumbs;
  }, [location.pathname, t]);

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  if (String(user.role).toLowerCase() !== 'admin') {
      return (
        <div className="flex flex-col items-center justify-center h-screen bg-gray-50 text-gray-800">
            <h1 className="text-2xl font-bold mb-2">403 - {t('common.forbidden')}</h1>
            <p className="mb-4">{t('messages.adminOnly')}</p>
            <Button onClick={() => navigate('/')}>{t('common.backToHome')}</Button>
        </div>
      );
  }

  const confirmLogout = () => {
    setShowLogoutConfirm(false);
    logout();
    navigate('/login');
  };

  return (
    <div className="flex min-h-screen bg-secondary">
      <AdminSidebar 
        collapsed={sidebarCollapsed} 
        onToggle={() => setSidebarCollapsed(!sidebarCollapsed)}
        filteredGroups={filteredNavGroups}
        onLogout={() => setShowLogoutConfirm(true)}
      />

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
               <AdminSidebar 
                collapsed={false} 
                mobile={true}
                onToggle={() => {}}
                filteredGroups={filteredNavGroups}
                onLogout={() => setShowLogoutConfirm(true)}
                onCloseMobile={() => setMobileMenuOpen(false)}
              />
            </motion.aside>
          </>
        )}
      </AnimatePresence>

      <div className={cn('flex-1 min-w-0 overflow-hidden transition-all duration-300', sidebarCollapsed ? 'lg:ml-20' : 'lg:ml-64')}>
        <AdminHeader 
            onMobileMenuOpen={() => setMobileMenuOpen(true)}
            breadcrumbs={breadcrumbs}
            onSearchClick={() => setShowSearchModal(true)}
            theme={theme}
            onToggleTheme={toggleTheme}
            isConnected={isConnected}
            onLogout={() => setShowLogoutConfirm(true)}
            onNavigate={navigate}
        />

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

      <AdminSearchModal 
        isOpen={showSearchModal}
        onClose={() => setShowSearchModal(false)}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
      />
    </div>
  );
};

export default AdminLayout;
