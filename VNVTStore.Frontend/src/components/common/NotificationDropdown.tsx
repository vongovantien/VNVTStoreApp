import React, { useState } from 'react';
import { Bell, Check, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui';
import { cn } from '@/utils/cn';
import { useClickOutside } from '@/hooks';
import { notificationService, type NotificationDto } from '@/services/notificationService';

interface NotificationDropdownProps {
  isConnected?: boolean;
  onNotificationClick?: (notification?: string) => void;
  className?: string;
}

export const NotificationDropdown: React.FC<NotificationDropdownProps> = ({
  isConnected = true,
  onNotificationClick,
  className
}) => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useClickOutside<HTMLDivElement>(() => setIsOpen(false));

  // Query unread count
  const { data: unreadCount = 0 } = useQuery({
    queryKey: ['unread-notifications-count'],
    queryFn: async () => {
      try {
        return await notificationService.getUnreadCount();
      } catch {
        return 0;
      }
    },
    refetchInterval: 30000,
  });

  // Query notifications list
  const { data: notifications = [] } = useQuery({
    queryKey: ['notifications'],
    queryFn: async () => {
      try {
        return await notificationService.getMyNotifications();
      } catch {
        return [];
      }
    },
    enabled: isOpen,
  });

  // Mark all read mutation
  const markAllReadMutation = useMutation({
    mutationFn: async () => {
      await notificationService.markAllAsRead();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
    },
  });

  // Mark single as read mutation
  const markAsReadMutation = useMutation({
    mutationFn: async (code: string) => {
      await notificationService.markAsRead(code);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
    },
  });

  // Delete notification mutation
  const deleteMutation = useMutation({
    mutationFn: async (code: string) => {
      await notificationService.deleteNotification(code);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
    },
  });

  const handleItemClick = (item: NotificationDto) => {
    if (!item.isRead) {
      markAsReadMutation.mutate(item.code);
    }
    if (item.link) {
      window.location.href = item.link;
    } else if (onNotificationClick) {
      onNotificationClick(item.title);
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
        title={isConnected ? t('header.notifications', 'Thông báo') : "Disconnected"}
      >
        <Bell size={20} className={cn(!isConnected && "opacity-50")} />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 flex h-4 min-w-4 px-1 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white shadow-sm">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </Button>

      {isOpen && (
         <div className="absolute right-0 mt-2 w-80 sm:w-96 bg-white dark:bg-slate-800 rounded-xl shadow-2xl border border-gray-100 dark:border-slate-700 z-50 overflow-hidden animate-fade-in">
           <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-slate-700 bg-gray-50/70 dark:bg-slate-850">
             <div className="flex items-center gap-2">
               <h3 className="font-semibold text-sm text-slate-900 dark:text-slate-100">
                 {t('header.notifications', 'Thông báo')}
               </h3>
               {unreadCount > 0 && (
                 <span className="px-2 py-0.5 rounded-full text-xs font-semibold bg-indigo-100 dark:bg-indigo-900/40 text-indigo-600 dark:text-indigo-400">
                   {unreadCount} mới
                 </span>
               )}
             </div>
             {unreadCount > 0 && (
               <button 
                 onClick={() => markAllReadMutation.mutate()}
                 disabled={markAllReadMutation.isPending}
                 className="flex items-center gap-1 text-xs text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 font-medium"
               >
                 <Check size={12} />
                 {t('common.markAllRead', 'Đọc tất cả')}
               </button>
             )}
           </div>
           
           <div className="max-h-[360px] overflow-y-auto divide-y divide-gray-100 dark:divide-slate-700">
              {notifications.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-10 px-4 text-center">
                  <div className="w-12 h-12 rounded-full bg-gray-100 dark:bg-slate-700 flex items-center justify-center mb-3">
                    <Bell size={20} className="text-gray-400 dark:text-slate-500" />
                  </div>
                  <p className="text-sm text-gray-500 dark:text-slate-400">
                    {t('common.noNotifications', 'Không có thông báo mới')}
                  </p>
                </div>
              ) : (
                notifications.slice(0, 8).map((note) => (
                  <div 
                    key={note.code} 
                    className={cn(
                      "px-4 py-3 hover:bg-gray-50 dark:hover:bg-slate-700/50 transition-colors cursor-pointer group flex items-start gap-3 relative",
                      !note.isRead && "bg-indigo-50/40 dark:bg-indigo-950/20"
                    )}
                    onClick={() => handleItemClick(note)}
                  >
                    {!note.isRead && (
                      <span className="absolute left-1 top-4 w-1.5 h-1.5 rounded-full bg-indigo-600" />
                    )}
                    <div className="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center text-indigo-600 dark:text-indigo-400 flex-shrink-0 mt-0.5">
                      <Bell size={14} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className={cn(
                        "text-xs font-semibold line-clamp-1 text-slate-800 dark:text-slate-200",
                        !note.isRead && "font-bold text-indigo-950 dark:text-indigo-200"
                      )}>
                        {note.title}
                      </p>
                      <p className="text-xs text-gray-500 dark:text-slate-400 line-clamp-2 mt-0.5">
                        {note.message}
                      </p>
                      <span className="text-[10px] text-gray-400 dark:text-slate-500 mt-1 block">
                        {new Date(note.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {new Date(note.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteMutation.mutate(note.code);
                      }}
                      className="opacity-0 group-hover:opacity-100 text-gray-400 hover:text-red-500 p-1 transition-all"
                      title="Xóa thông báo"
                    >
                      <Trash2 size={13} />
                    </button>
                  </div>
                ))
              )}
           </div>
           
           {onNotificationClick && (
             <div className="p-2 border-t border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-850 text-center">
                <button 
                  className="text-xs text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 font-semibold w-full py-1"
                  onClick={() => {
                     setIsOpen(false);
                     onNotificationClick();
                  }}
                >
                  {t('common.viewAll', 'Xem tất cả thông báo')} →
                </button>
             </div>
           )}
         </div>
      )}
    </div>
  );
};

export default NotificationDropdown;
