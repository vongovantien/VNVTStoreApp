import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5176/notificationHub'; // Generic Hub

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private callbacks: Record<string, ((message: any) => void)[]> = {};

    public async startConnection() {
        if (this.connection?.state === signalR.HubConnectionState.Connected) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveOrderNotification', (message: string) => {
            this.notifyListeners('ReceiveOrderNotification', message);
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            // Retry logic could go here
        }
    }

    public on(event: string, callback: (message: any) => void) {
        if (!this.callbacks[event]) {
            this.callbacks[event] = [];
        }
        this.callbacks[event].push(callback);
    }

    public off(event: string, callback: (message: any) => void) {
        if (!this.callbacks[event]) return;
        this.callbacks[event] = this.callbacks[event].filter(cb => cb !== callback);
    }

    private notifyListeners(event: string, message: any) {
        if (this.callbacks[event]) {
            this.callbacks[event].forEach(callback => callback(message));
        }
    }
}

export const signalRService = new SignalRService();
