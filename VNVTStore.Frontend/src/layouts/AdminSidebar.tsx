import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  FileText,
  Folder,
  Building2,
  Tag,
  Ruler,
  Ticket,
  Star,
  Shield,
  ChevronRight,
  ChevronLeft,
  LogOut,
} from 'lucide-react';
import { cn } from '@/utils/cn';

export interface NavItem {
  path: string;
  icon: React.ElementType;
  label: string;
  code: string;
  end?: boolean;
}

export interface NavGroup {
  title: string;
  items: NavItem[];
}

export const navGroups: NavGroup[] = [
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
      { path: '/admin/settings', icon: Shield, label: 'admin.sidebar.settings', code: 'SETTINGS', end: false },
      { path: '/admin/system-configs', icon: LayoutDashboard, label: 'admin.sidebar.systemConfigs', code: 'SETTINGS', end: false },
      { path: '/admin/system-secrets', icon: Shield, label: 'admin.sidebar.systemSecrets', code: 'SETTINGS', end: false },
      { path: '/admin/audit-logs', icon: FileText, label: 'admin.sidebar.auditLogs', code: 'AUDIT_LOGS', end: false },
      { path: '/admin/roles', icon: Shield, label: 'admin.sidebar.roles', code: 'ROLES', end: false },
    ]
  }
];

interface AdminSidebarProps {
    collapsed: boolean;
    onToggle: () => void;
    filteredGroups: NavGroup[];
    onLogout: () => void;
    mobile?: boolean;
    onCloseMobile?: () => void;
}

export const AdminSidebar = ({ 
    collapsed, 
    onToggle, 
    filteredGroups, 
    onLogout, 
    mobile, 
    onCloseMobile 
}: AdminSidebarProps) => {
    const { t } = useTranslation();

    const sidebarContent = (
        <>
            <div className="flex items-center justify-between h-16 px-4 border-b border-gray-800 shrink-0">
                <AnimatePresence mode="wait">
                    {(!collapsed || mobile) && (
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
                {!mobile && (
                    <button
                        onClick={onToggle}
                        className="p-2 rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        {collapsed ? <ChevronRight size={20} /> : <ChevronLeft size={20} />}
                    </button>
                )}
                {mobile && (
                    <button
                        onClick={onCloseMobile}
                        className="ml-auto p-2 text-gray-400 hover:text-white"
                    >
                        <ChevronLeft size={20} />
                    </button>
                )}
            </div>

            <nav className="flex-1 min-h-0 overflow-y-auto custom-scrollbar-dark p-4 space-y-6">
                {filteredGroups.map((group, index) => (
                    <div key={index}>
                        {(!collapsed || mobile) && (
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
                                    onClick={mobile ? onCloseMobile : undefined}
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
                                            width: (collapsed && !mobile) ? 0 : 'auto',
                                            opacity: (collapsed && !mobile) ? 0 : 1,
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

            <div className="p-4 border-t border-gray-800 shrink-0">
                <button
                    onClick={onLogout}
                    className={cn(
                        'flex items-center gap-3 w-full px-4 py-3 text-gray-400 rounded-lg hover:text-white hover:bg-gray-800 transition-all'
                    )}
                >
                    <LogOut size={20} />
                    {(!collapsed || mobile) && <span>{t('common.logout')}</span>}
                </button>
            </div>
        </>
    );

    if (mobile) return sidebarContent;

    return (
        <aside
            className={cn(
                'fixed left-0 top-0 z-40 h-screen bg-gray-900 text-white transition-all duration-300 flex flex-col',
                collapsed ? 'w-20' : 'w-64',
                'hidden lg:block'
            )}
        >
            {sidebarContent}
        </aside>
    );
};
