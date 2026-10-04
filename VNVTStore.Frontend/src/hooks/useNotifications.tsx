import { useEffect, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { useAuthStore, useToastStore } from '@/store';

export interface NotificationPayload {
    id?: string;
    type: 'order' | 'quote' | 'delivery' | 'system';
    title: string;
    message: string;
    link?: string;
    createdAt?: string;
    isRead?: boolean;
}

/**
 * Hook quản lý realtime notifications qua SignalR
 */
export function useNotifications() {
    const queryClient = useQueryClient();
    const { user, token } = useAuthStore();
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!token || !user) return;

        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/hubs/notifications`, {
                accessTokenFactory: () => token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Event handlers
        hubConnection.on('ReceiveNotification', (payload: NotificationPayload) => {
            console.log('[SignalR] Notification received:', payload);

            // Show toast using native useToastStore
            useToastStore.getState().info(`${payload.title}: ${payload.message}`);

            // Invalidate notifications query để refresh danh sách
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            queryClient.invalidateQueries({ queryKey: ['unread-notifications-count'] });
        });

        hubConnection.onreconnecting(() => {
            console.log('[SignalR] Reconnecting...');
            setIsConnected(false);
        });

        hubConnection.onreconnected(() => {
            console.log('[SignalR] Reconnected');
            setIsConnected(true);
        });

        hubConnection.onclose(() => {
            console.log('[SignalR] Connection closed');
            setIsConnected(false);
        });

        let isMounted = true;

        // Start connection
        hubConnection
            .start()
            .then(() => {
                if (!isMounted) {
                    hubConnection.stop();
                    return;
                }
                console.log('[SignalR] Connected to notification hub');
                setIsConnected(true);
            })
            .catch((err) => {
                if (!isMounted) return;
                console.error('[SignalR] Connection failed:', err);
                setIsConnected(false);
            });

        setConnection(hubConnection);

        return () => {
            isMounted = false;
            hubConnection.stop();
        };
    }, [token, user, queryClient]);

    return {
        connection,
        isConnected,
    };
}
