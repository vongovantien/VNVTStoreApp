import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5176/notificationHub';

export interface SignalRNotification {
    Key?: string;
    Args?: Record<string, unknown>;
    Message?: string;
}

type SignalRCallback = (data: string | SignalRNotification) => void;

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private callbacks: Record<string, SignalRCallback[]> = {};

    public async startConnection() {
        if (this.connection?.state === signalR.HubConnectionState.Connected) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveOrderNotification', (message: string) => {
            this.notifyListeners('ReceiveOrderNotification', message);
        });

        this.connection.on('ReceiveSystemNotification', (data: string | SignalRNotification) => {
            this.notifyListeners('ReceiveSystemNotification', data);
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
        }
    }

    public on(event: string, callback: SignalRCallback) {
        if (!this.callbacks[event]) {
            this.callbacks[event] = [];
        }
        this.callbacks[event].push(callback);
    }

    public off(event: string, callback: SignalRCallback) {
        if (!this.callbacks[event]) return;
        this.callbacks[event] = this.callbacks[event].filter(cb => cb !== callback);
    }

    private notifyListeners(event: string, message: string | SignalRNotification) {
        if (this.callbacks[event]) {
            this.callbacks[event].forEach(callback => callback(message));
        }
    }
}

export const signalRService = new SignalRService();
