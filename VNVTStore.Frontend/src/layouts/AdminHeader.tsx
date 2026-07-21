import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Menu,
  Home,
  ChevronRight,
  Search,
  Command,
  Moon,
  Sun,
  ExternalLink,
  User as UserIcon,
  HelpCircle,
  FileKey,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { NotificationDropdown, UserMenu, LanguageSwitcher } from '@/components/common';

interface AdminHeaderProps {
    onMobileMenuOpen: () => void;
    breadcrumbs: { label: string; path: string }[];
    onSearchClick: () => void;
    theme: string;
    onToggleTheme: () => void;
    isConnected: boolean;
    onLogout: () => void;
    onNavigate: (path: string) => void;
}

export const AdminHeader = ({
    onMobileMenuOpen,
    breadcrumbs,
    onSearchClick,
    theme,
    onToggleTheme,
    isConnected,
    onLogout,
    onNavigate
}: AdminHeaderProps) => {
    const { t } = useTranslation();

    return (
        <header className="sticky top-0 z-30 bg-primary shadow-sm">
            <div className="flex items-center justify-between h-16 px-4 lg:px-6">
                <button
                    className="p-2 rounded-lg hover:bg-hover lg:hidden"
                    onClick={onMobileMenuOpen}
                >
                    <Menu size={24} />
                </button>

                {/* Breadcrumbs */}
                <div className="hidden lg:flex items-center gap-1 text-sm">
                    <Home size={14} className="text-tertiary" />
                    {breadcrumbs.map((crumb, index) => (
                        <div key={crumb.path} className="flex items-center gap-1">
                            <ChevronRight size={14} className="text-tertiary" />
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

                {/* Search */}
                <div className="hidden md:block flex-1 max-w-sm mx-4">
                    <button
                        onClick={onSearchClick}
                        className="w-full flex items-center gap-3 px-4 py-2 bg-secondary rounded-lg border border-transparent hover:border-indigo-500 transition-all group"
                    >
                        <Search size={18} className="text-tertiary group-hover:text-indigo-500" />
                        <span className="flex-1 text-left text-sm text-tertiary">{t('common.search')}</span>
                        <kbd className="hidden sm:inline-flex items-center gap-1 px-2 py-0.5 text-[10px] font-medium text-tertiary bg-primary rounded border">
                            <Command size={10} /> K
                        </kbd>
                    </button>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2">
                    <LanguageSwitcher variant="ghost" align="right" />

                    <Button variant="ghost" size="sm" onClick={onToggleTheme}>
                        {theme === 'light' ? <Moon size={20} /> : <Sun size={20} />}
                    </Button>

                    <NotificationDropdown 
                        isConnected={isConnected}
                        onNotificationClick={() => onNavigate('/admin/orders')}
                    />

                    <UserMenu 
                        onLogout={onLogout}
                        items={[
                            { label: t('admin.viewStore'), icon: ExternalLink, onClick: () => window.open('/', '_blank') },
                            { label: t('admin.userMenu.accountSettings'), icon: UserIcon, link: '/admin/settings' },
                            { label: t('admin.userMenu.support'), icon: HelpCircle },
                            { label: t('admin.userMenu.license'), icon: FileKey }
                        ]}
                    />
                </div>
            </div>
        </header>
    );
};
