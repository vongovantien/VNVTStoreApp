import React, { useState, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronDown, LogOut, LucideIcon } from 'lucide-react';
import { cn } from '@/utils/cn';
import { useAuthStore } from '@/store';
import { useClickOutside } from '@/hooks';
import { UserAvatar } from './UserAvatar';

export interface UserMenuItem {
  label: string;
  icon: LucideIcon;
  link?: string;
  onClick?: () => void;
  className?: string;
}

interface UserMenuProps {
  items: UserMenuItem[];
  className?: string;
  onLogout: () => void;
}

export const UserMenu: React.FC<UserMenuProps> = ({
  items,
  className,
  onLogout
}) => {
  const { t } = useTranslation();
  const { user, isAuthenticated } = useAuthStore();
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useClickOutside<HTMLDivElement>(() => setIsOpen(false));

  if (!isAuthenticated) return null;

  return (
    <div className={cn("relative z-50", className)} ref={menuRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-3 hover:opacity-80 transition-opacity pl-4 border-l border-secondary/10 ml-3"
      >
        <UserAvatar size="sm" />
        <div className="hidden sm:block text-left">
          <p className="text-sm font-bold text-primary leading-tight truncate max-w-[120px]">
            {user?.fullName || (user?.email ? user.email.split('@')[0] : 'User')}
          </p>
          <p className="text-[10px] text-tertiary truncate max-w-[120px] font-medium opacity-80">
            {user?.email}
          </p>
        </div>
        <ChevronDown 
          size={14} 
          className={cn("text-tertiary transition-transform duration-200", isOpen && "rotate-180")} 
        />
      </button>

      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 10, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 10, scale: 0.95 }}
            className="absolute top-full right-0 mt-3 bg-primary rounded-xl shadow-2xl border border-secondary/5 p-1 min-w-[240px] overflow-hidden"
          >
            {/* Header Area */}
            <div className="flex items-center gap-3 px-4 py-3 border-b border-secondary/5 mb-1 bg-secondary/10">
              <UserAvatar size="md" />
              <div className="flex-1 min-w-0">
                <p className="font-bold text-sm truncate text-primary">
                  {user?.fullName || 'User'}
                </p>
                <p className="text-[11px] text-tertiary truncate font-medium">
                  {user?.email}
                </p>
              </div>
            </div>

            {/* Menu Items */}
            <div className="p-1 space-y-0.5">
              {items.map((item, index) => {
                const content = (
                  <>
                    <item.icon size={18} />
                    <span>{item.label}</span>
                  </>
                );

                const baseClasses = cn(
                  "flex items-center gap-3 w-full px-3 py-2 text-sm text-secondary hover:text-primary hover:bg-hover rounded-lg transition-colors font-medium text-left",
                  item.className
                );

                if (item.link) {
                  return (
                    <Link
                      key={index}
                      to={item.link}
                      onClick={() => {
                        setIsOpen(false);
                        item.onClick?.();
                      }}
                      className={baseClasses}
                    >
                      {content}
                    </Link>
                  );
                }

                return (
                  <button
                    key={index}
                    onClick={() => {
                      setIsOpen(false);
                      item.onClick?.();
                    }}
                    className={baseClasses}
                  >
                    {content}
                  </button>
                );
              })}

              <hr className="my-1 border-secondary/5" />

              <button
                onClick={() => {
                  setIsOpen(false);
                  onLogout();
                }}
                className="flex items-center gap-3 w-full text-left px-3 py-2 text-sm text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 rounded-lg transition-colors font-medium"
              >
                <LogOut size={18} />
                {t('common.logout')}
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};
