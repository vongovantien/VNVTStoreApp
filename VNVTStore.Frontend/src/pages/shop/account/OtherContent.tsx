import { useState } from 'react';
import { Switch, Button } from '@/components/ui';
import { Bell, Globe } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useToast } from '@/store';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService, type NotificationDto } from '@/services/notificationService';
import { Check, Trash2, Package, ShoppingCart, MessageSquare, AlertCircle } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { vi as viLocale } from 'date-fns/locale';

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

export const NotificationsContent = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState<'all' | 'unread'>('all');

    const { data: notifications = [], isLoading } = useQuery({
        queryKey: ['notifications'],
        queryFn: async () => {
            return await notificationService.getMyNotifications();
        },
    });

    const markAsReadMutation = useMutation({
        mutationFn: async (code: string) => {
            await notificationService.markAsRead(code);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
        },
    });

    const markAllAsReadMutation = useMutation({
        mutationFn: async () => {
            await notificationService.markAllAsRead();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
        },
    });

    const deleteMutation = useMutation({
        mutationFn: async (code: string) => {
            await notificationService.deleteNotification(code);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
        },
    });

    const filtered = filter === 'unread' 
        ? notifications.filter(n => !n.isRead) 
        : notifications;
    const unreadCount = notifications.filter(n => !n.isRead).length;

    return (
        <div className="bg-primary rounded-2xl p-6 border shadow-sm min-h-[500px]">
            <div className="flex flex-wrap items-center justify-between gap-4 pb-5 border-b mb-6">
                <div>
                    <h2 className="text-xl font-bold flex items-center gap-2 text-primary">
                        <Bell className="text-indigo-600" size={24} />
                        {t('common.account.myNotifications', 'Trung tâm thông báo')}
                    </h2>
                    <p className="text-xs text-secondary mt-1">
                        {unreadCount > 0 ? `Bạn có ${unreadCount} thông báo chưa đọc` : 'Bạn đã đọc hết mọi thông báo'}
                    </p>
                </div>

                <div className="flex items-center gap-3">
                    <div className="flex bg-secondary p-1 rounded-xl text-xs font-semibold">
                        <button
                            onClick={() => setFilter('all')}
                            className={`px-3 py-1.5 rounded-lg transition-all ${
                                filter === 'all' ? 'bg-primary shadow text-primary font-bold' : 'text-secondary hover:text-primary'
                            }`}
                        >
                            Tất cả ({notifications.length})
                        </button>
                        <button
                            onClick={() => setFilter('unread')}
                            className={`px-3 py-1.5 rounded-lg transition-all ${
                                filter === 'unread' ? 'bg-primary shadow text-primary font-bold' : 'text-secondary hover:text-primary'
                            }`}
                        >
                            Chưa đọc ({unreadCount})
                        </button>
                    </div>

                    {unreadCount > 0 && (
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => markAllAsReadMutation.mutate()}
                            disabled={markAllAsReadMutation.isPending}
                            className="text-xs flex items-center gap-1.5"
                        >
                            <Check size={14} />
                            Đánh dấu đã đọc tất cả
                        </Button>
                    )}
                </div>
            </div>

            {isLoading ? (
                <div className="py-20 text-center text-secondary">
                    <div className="animate-spin w-8 h-8 border-2 border-indigo-600 border-t-transparent rounded-full mx-auto mb-3" />
                    Đang tải thông báo...
                </div>
            ) : filtered.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-20 text-center">
                    <div className="w-20 h-20 bg-secondary rounded-full flex items-center justify-center mb-4">
                        <Bell className="text-tertiary" size={36} />
                    </div>
                    <h3 className="text-base font-semibold text-primary">
                        {filter === 'unread' ? 'Không có thông báo chưa đọc nào' : t('common.account.noNotifications', 'Chưa có thông báo nào')}
                    </h3>
                    <p className="text-xs text-secondary mt-1">Thông báo về đơn hàng và ưu đãi sẽ xuất hiện tại đây</p>
                </div>
            ) : (
                <div className="space-y-3">
                    {filtered.map((item) => {
                        const typeKey = (item.type || 'INFO').toLowerCase();
                        const Icon = ICON_MAP[typeKey] || ICON_MAP[item.type] || Bell;
                        const colorClass = COLOR_MAP[typeKey] || COLOR_MAP[item.type] || 'bg-indigo-600';

                        return (
                            <div
                                key={item.code}
                                onClick={() => {
                                    if (!item.isRead) markAsReadMutation.mutate(item.code);
                                    if (item.link) window.location.href = item.link;
                                }}
                                className={`p-4 rounded-xl border transition-all cursor-pointer hover:shadow-md flex items-start gap-4 relative group ${
                                    !item.isRead 
                                        ? 'bg-indigo-50/50 dark:bg-indigo-950/20 border-indigo-200 dark:border-indigo-800' 
                                        : 'bg-primary border-border hover:border-indigo-300'
                                }`}
                            >
                                {!item.isRead && (
                                    <span className="w-2.5 h-2.5 rounded-full bg-indigo-600 absolute top-5 left-2" />
                                )}

                                <div className={`w-10 h-10 rounded-xl ${colorClass} text-white flex items-center justify-center flex-shrink-0 ml-2`}>
                                    <Icon size={20} />
                                </div>

                                <div className="flex-1 min-w-0">
                                    <div className="flex items-center justify-between gap-2">
                                        <h4 className={`text-sm font-semibold truncate ${!item.isRead ? 'text-indigo-950 dark:text-indigo-200 font-bold' : 'text-primary'}`}>
                                            {item.title}
                                        </h4>
                                        <span className="text-[11px] text-tertiary flex-shrink-0">
                                            {formatDistanceToNow(new Date(item.createdAt), { addSuffix: true, locale: viLocale })}
                                        </span>
                                    </div>
                                    <p className="text-xs text-secondary mt-1 line-clamp-2">
                                        {item.message}
                                    </p>
                                    {item.link && (
                                        <span className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold inline-block mt-2 group-hover:underline">
                                            Xem chi tiết →
                                        </span>
                                    )}
                                </div>

                                <button
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        deleteMutation.mutate(item.code);
                                    }}
                                    className="p-1.5 opacity-0 group-hover:opacity-100 hover:bg-red-500/10 rounded-lg text-red-500 transition-all"
                                    title="Xóa thông báo"
                                >
                                    <Trash2 size={16} />
                                </button>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};

interface SettingsState {
    emailNotif: boolean;
    orderNotif: boolean;
    promoNotif: boolean;
    darkMode: boolean;
    language: string;
}

export const SettingsContent = () => {
    const { t, i18n } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    
    // Preferences State
    const [settings, setSettings] = useState<SettingsState>(() => {
        const defaults = {
            emailNotif: true,
            orderNotif: true,
            promoNotif: true,
            darkMode: false,
            language: 'vi'
        };
        const saved = localStorage.getItem('user_settings');
        if (saved) {
            try {
                return { ...defaults, ...JSON.parse(saved) };
            } catch (e) { 
                console.error('Failed to parse settings', e); 
                return defaults;
            }
        }
        return defaults;
    });

    const handleSave = () => {
        setLoading(true);
        // Simulate API call
        setTimeout(() => {
            localStorage.setItem('user_settings', JSON.stringify(settings));
            // Apply side effects
            if (settings.language !== i18n.language) {
                i18n.changeLanguage(settings.language);
            }
            // Dark mode toggle would go here if implemented globally
            
            toast.success(t('common.account.settingsPage.updateSuccess'));
            setLoading(false);
        }, 800);
    };

    return (
        <div className="space-y-6">
            <div className="bg-primary rounded-xl p-6 border shadow-sm">
                <h2 className="text-xl font-bold mb-6">{t('common.account.settings')}</h2>
                
                <div className="space-y-8">
                    {/* Notifications Group */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Bell size={18} /> {t('common.account.settingsPage.notifications')}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <Switch
                                label={t('common.account.settingsPage.orderNotif')}
                                description={t('common.account.settingsPage.orderNotifDesc')}
                                checked={settings.orderNotif}
                                onChange={(v) => setSettings(s => ({ ...s, orderNotif: v }))}
                            />
                            <div className="h-px bg-secondary/10" />
                            <Switch
                                label={t('common.account.settingsPage.promoNotif')}
                                description={t('common.account.settingsPage.promoNotifDesc')}
                                checked={settings.emailNotif}
                                onChange={(v) => setSettings(s => ({ ...s, emailNotif: v }))}
                            />
                        </div>
                    </div>

                    {/* App Preferences */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Globe size={18} /> {t('common.account.settingsPage.preferences')}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="font-medium">{t('common.account.settingsPage.language')}</p>
                                    <p className="text-xs text-secondary">{t('common.account.settingsPage.languageDesc')}</p>
                                </div>
                                <div className="flex gap-2">
                                    <button 
                                        type="button"
                                        onClick={() => setSettings(s => ({ ...s, language: 'vi' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
                                            settings.language === 'vi' 
                                                ? 'bg-accent text-accent-foreground border-accent shadow-sm' 
                                                : 'bg-secondary text-secondary border-transparent hover:bg-hover'
                                        }`}
                                    >
                                        {t('common.account.settingsPage.vietnamese')}
                                    </button>
                                    <button 
                                        type="button"
                                        onClick={() => setSettings(s => ({ ...s, language: 'en' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
                                            settings.language === 'en' 
                                                ? 'bg-accent text-accent-foreground border-accent shadow-sm' 
                                                : 'bg-secondary text-secondary border-transparent hover:bg-hover'
                                        }`}
                                    >
                                        {t('common.account.settingsPage.english')}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <Button onClick={handleSave} isLoading={loading}>
                            {t('common.account.settingsPage.saveChanges')}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
};

