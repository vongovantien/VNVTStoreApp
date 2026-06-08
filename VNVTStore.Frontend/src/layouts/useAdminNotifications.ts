import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useSignalR } from '@/hooks/useSignalR';
import { useToastStore, useNotificationStore } from '@/store';

export const useAdminNotifications = () => {
    const { t } = useTranslation();
    const { on, isConnected } = useSignalR();
    const { info } = useToastStore();
    const { addNotification } = useNotificationStore();

    useEffect(() => {
        // Listen for new orders
        const cleanupOrder = on('ReceiveOrderNotification', (data: any) => {
           const message = typeof data === 'string' ? data : data?.Message || t('admin.notifications.newOrder');
           info(message);
           addNotification(message);
           
           const audio = new Audio('/notification.mp3');
           audio.play().catch(e => console.log('Audio play failed', e)); 
        });

        return () => {
            cleanupOrder();
        };
    }, [on, info, addNotification, t]);

    return { isConnected };
};
