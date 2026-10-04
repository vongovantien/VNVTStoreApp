import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Bell, Check, Trash2, Package, ShoppingCart, MessageSquare, AlertCircle } from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';
import { notificationService, type NotificationDto } from '@/services/notificationService';
import { useToastStore } from '@/store';

interface NotificationDrawerProps {
    isOpen: boolean;
    onClose: () => void;
}

const ICON_MAP: Record<string, React.ElementType> = {
    order: ShoppingCart,
    quote: MessageSquare,
    delivery: Package,
    system: AlertCircle,
    INFO: Bell,
};

const COLOR_MAP: Record<string, string> = {
    order: 'bg-blue-500',
    quote: 'bg-purple-500',
    delivery: 'bg-orange-500',
    system: 'bg-gray-500',
    INFO: 'bg-indigo-500',
};

export const NotificationDrawer = ({ isOpen, onClose }: NotificationDrawerProps) => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState<'all' | 'unread'>('all');

    // Fetch notifications
    const { data: notificationsRes, isLoading } = useQuery({
        queryKey: ['notifications'],
        queryFn: async () => {
            return await notificationService.getMyNotifications();
        },
        enabled: isOpen,
        refetchInterval: 30000,
    });

    const allNotifications: NotificationDto[] = notificationsRes || [];
    const notifications = filter === 'unread' 
        ? allNotifications.filter(n => !n.isRead) 
        : allNotifications;
    const unreadCount = allNotifications.filter(n => !n.isRead).length;

    // Mark as read mutation
    const markAsReadMutation = useMutation({
        mutationFn: async (code: string) => {
            await notificationService.markAsRead(code);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
        },
    });

    // Mark all as read mutation
    const markAllAsReadMutation = useMutation({
        mutationFn: async () => {
            await notificationService.markAllAsRead();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
            useToastStore.getState().success('Đã đánh dấu tất cả là đã đọc');
        },
    });

    // Delete mutation
    const deleteMutation = useMutation({
        mutationFn: async (code: string) => {
            await notificationService.deleteNotification(code);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
            useToastStore.getState().success('Đã xóa thông báo');
        },
    });

    const handleNotificationClick = (notification: NotificationDto) => {
        if (!notification.isRead) {
            markAsReadMutation.mutate(notification.code);
        }
        if (notification.link) {
            window.location.href = notification.link;
            onClose();
        }
    };

    return (
        <AnimatePresence>
            {isOpen && (
                <>
                    {/* Backdrop */}
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        onClick={onClose}
                        className="fixed inset-0 bg-black/50 z-[998]"
                    />

                    {/* Drawer */}
                    <motion.div
                        initial={{ x: '100%' }}
                        animate={{ x: 0 }}
                        exit={{ x: '100%' }}
                        transition={{ type: 'spring', damping: 30, stiffness: 300 }}
                        className="fixed right-0 top-0 h-full w-full sm:w-96 bg-primary shadow-2xl z-[999] flex flex-col"
                    >
                        {/* Header */}
                        <div className="flex items-center justify-between p-4 border-b border-border bg-gradient-to-r from-indigo-500 to-purple-500">
                            <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
                                    <Bell size={20} className="text-white" />
                                </div>
                                <div>
                                    <h2 className="text-lg font-bold text-white">Thông báo</h2>
                                    {unreadCount > 0 && (
                                        <p className="text-xs text-white/80">{unreadCount} chưa đọc</p>
                                    )}
                                </div>
                            </div>
                            <button
                                onClick={onClose}
                                className="w-8 h-8 rounded-lg bg-white/20 hover:bg-white/30 flex items-center justify-center text-white transition-colors"
                            >
                                <X size={20} />
                            </button>
                        </div>

                        {/* Filter & Actions */}
                        <div className="flex items-center justify-between p-4 border-b border-border bg-secondary/30">
                            <div className="flex gap-2">
                                <button
                                    onClick={() => setFilter('all')}
                                    className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                                        filter === 'all'
                                            ? 'bg-accent text-white'
                                            : 'bg-secondary hover:bg-secondary/80'
                                    }`}
                                >
                                    Tất cả
                                </button>
                                <button
                                    onClick={() => setFilter('unread')}
                                    className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                                        filter === 'unread'
                                            ? 'bg-accent text-white'
                                            : 'bg-secondary hover:bg-secondary/80'
                                    }`}
                                >
                                    Chưa đọc
                                </button>
                            </div>
                            {unreadCount > 0 && (
                                <button
                                    onClick={() => markAllAsReadMutation.mutate()}
                                    disabled={markAllAsReadMutation.isPending}
                                    className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-accent hover:text-accent/80 transition-colors"
                                >
                                    <Check size={14} />
                                    Đọc tất cả
                                </button>
                            )}
                        </div>

                        {/* Notifications List */}
                        <div className="flex-1 overflow-y-auto">
                            {isLoading ? (
                                <div className="p-12 text-center">
                                    <div className="animate-spin w-8 h-8 border-2 border-accent border-t-transparent rounded-full mx-auto mb-3" />
                                    <p className="text-sm text-tertiary">Đang tải...</p>
                                </div>
                            ) : notifications.length === 0 ? (
                                <div className="p-12 text-center">
                                    <Bell size={48} className="mx-auto text-tertiary/30 mb-3" />
                                    <p className="text-sm text-tertiary">
                                        {filter === 'unread' ? 'Không có thông báo chưa đọc' : 'Chưa có thông báo nào'}
                                    </p>
                                </div>
                            ) : (
                                <div className="divide-y divide-border">
                                    {notifications.map((notification) => {
                                        const typeKey = (notification.type || 'INFO').toLowerCase();
                                        const Icon = ICON_MAP[typeKey] || ICON_MAP[notification.type] || Bell;
                                        const colorClass = COLOR_MAP[typeKey] || COLOR_MAP[notification.type] || 'bg-gray-500';
                                        
                                        return (
                                            <motion.div
                                                key={notification.code}
                                                initial={{ opacity: 0, x: 20 }}
                                                animate={{ opacity: 1, x: 0 }}
                                                className={`p-4 hover:bg-secondary/50 transition-colors relative cursor-pointer ${
                                                    !notification.isRead ? 'bg-accent/5' : ''
                                                }`}
                                                onClick={() => handleNotificationClick(notification)}
                                            >
                                                {!notification.isRead && (
                                                    <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-12 bg-accent rounded-r-full" />
                                                )}
                                                
                                                <div className="flex gap-3">
                                                    <div className={`w-10 h-10 rounded-full ${colorClass} flex items-center justify-center flex-shrink-0`}>
                                                        <Icon size={18} className="text-white" />
                                                    </div>
                                                    
                                                    <div className="flex-1 min-w-0">
                                                        <div className="flex items-start justify-between gap-2 mb-1">
                                                            <h3 className={`text-sm font-semibold ${!notification.isRead ? 'text-primary' : 'text-tertiary'}`}>
                                                                {notification.title}
                                                            </h3>
                                                            <button
                                                                onClick={(e) => {
                                                                    e.stopPropagation();
                                                                    deleteMutation.mutate(notification.code);
                                                                }}
                                                                className="p-1 hover:bg-red-500/10 rounded text-red-500 transition-colors"
                                                            >
                                                                <Trash2 size={14} />
                                                            </button>
                                                        </div>
                                                        
                                                        <p className="text-sm text-tertiary mb-2 line-clamp-2">
                                                            {notification.message}
                                                        </p>
                                                        
                                                        <div className="flex items-center justify-between">
                                                            <p className="text-xs text-tertiary">
                                                                {formatDistanceToNow(new Date(notification.createdAt), {
                                                                    addSuffix: true,
                                                                    locale: vi,
                                                                })}
                                                            </p>
                                                            
                                                            {notification.link && (
                                                                <span className="text-xs text-accent hover:text-accent/80 font-medium">
                                                                    Xem chi tiết →
                                                                </span>
                                                            )}
                                                        </div>
                                                    </div>
                                                </div>
                                            </motion.div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </motion.div>
                </>
            )}
        </AnimatePresence>
    );
};
