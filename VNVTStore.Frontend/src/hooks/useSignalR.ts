import { useEffect, useState, useCallback } from 'react';
import { signalRService, SignalRNotification, ConnectionStatus } from '@/services/signalrService';

export const useSignalR = () => {
    const [status, setStatus] = useState<ConnectionStatus>(signalRService.getStatus());

    useEffect(() => {
        signalRService.setStatusCallback((newStatus) => {
            setStatus(newStatus);
        });

        const init = async () => {
            await signalRService.startConnection();
        };

        if (signalRService.getStatus() === 'Disconnected') {
            init();
        }

        return () => {
            signalRService.setStatusCallback(() => { });
        };
    }, []);

    const on = useCallback((event: string, callback: (data: string | SignalRNotification) => void) => {
        signalRService.on(event, callback);
        return () => signalRService.off(event, callback);
    }, []);

    return {
        status,
        on,
        isConnected: status === 'Connected'
    };
};
