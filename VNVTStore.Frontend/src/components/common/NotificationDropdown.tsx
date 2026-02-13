import React, { useState } from 'react';
import { Bell } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { cn } from '@/utils/cn';
import { useNotificationStore } from '@/store';
import { useClickOutside } from '@/hooks';

interface NotificationDropdownProps {
  isConnected?: boolean;
  onNotificationClick?: (notification: string) => void;
  className?: string;
}

export const NotificationDropdown: React.FC<NotificationDropdownProps> = ({
  isConnected = true,
  onNotificationClick,
  className
}) => {
  const { t } = useTranslation();
  const { notifications, unreadCount, markAllRead, clearNotifications } = useNotificationStore();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useClickOutside<HTMLDivElement>(() => setIsOpen(false));

  const handleNotificationClick = (note: string) => {
    if (onNotificationClick) {
      onNotificationClick(note);
    }
    setIsOpen(false);
  };

  return (
    <div className={cn("relative", className)} ref={dropdownRef}>
      <Button 
        variant="ghost" 
        size="sm" 
        className="relative"
        onClick={() => setIsOpen(!isOpen)}
        title={isConnected ? t('header.notifications') : "Disconnected"}
      >
        <Bell size={20} className={cn(!isConnected && "opacity-50")} />
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 flex h-3 w-3">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-error opacity-75"></span>
            <span className="relative inline-flex rounded-full h-3 w-3 bg-error"></span>
          </span>
        )}
      </Button>

      {isOpen && (
         <div className="absolute right-0 mt-2 w-80 bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-gray-100 dark:border-slate-700 z-50 overflow-hidden animate-fade-in">
           <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-800/50">
             <h3 className="font-semibold text-sm text-slate-900 dark:text-slate-100">
               {t('header.notifications')}
             </h3>
             {unreadCount > 0 && (
               <button 
                 onClick={() => markAllRead()}
                 className="text-xs text-blue-600 dark:text-blue-400 hover:text-blue-700 font-medium"
               >
                 {t('common.active') || 'Mark read'}
               </button>
             )}
           </div>
           
           <div className="max-h-[350px] overflow-y-auto custom-scrollbar-dark">
              {notifications.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-8 px-4 text-center">
                  <div className="w-12 h-12 rounded-full bg-gray-100 dark:bg-slate-700 flex items-center justify-center mb-3">
                    <Bell size={20} className="text-gray-400 dark:text-slate-500" />
                  </div>
                  <p className="text-sm text-gray-500 dark:text-slate-400">
                    {t('common.noNotifications') || 'No new notifications'}
                  </p>
                </div>
              ) : (
                <div className="divide-y divide-gray-100 dark:divide-slate-700">
                  {notifications.map((note, idx) => (
                    <div 
                      key={idx} 
                      className="px-4 py-3 hover:bg-tertiary transition-colors cursor-pointer group"
                      onClick={() => handleNotificationClick(note)}
                    >
                      <div className="flex gap-3">
                        <div className="mt-1 flex-shrink-0">
                          <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-400">
                            <Bell size={14} />
                          </div>
                        </div>
                        <div>
                          <p className="text-sm text-gray-800 dark:text-slate-200 line-clamp-2">
                            {note}
                          </p>
                          <p className="text-xs text-gray-400 mt-1 group-hover:text-blue-500 transition-colors">
                            {t('common.viewDetails')}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
           </div>
           
           <div className="p-2 border-t border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-800/50 text-center">
              <button 
                className="text-xs text-gray-500 hover:text-gray-900 dark:text-slate-400 dark:hover:text-slate-200 transition-colors w-full py-1"
                onClick={() => {
                   clearNotifications();
                   setIsOpen(false);
                }}
              >
                {t('common.clearAll') || 'Clear all'}
              </button>
           </div>
         </div>
      )}
    </div>
  );
};
