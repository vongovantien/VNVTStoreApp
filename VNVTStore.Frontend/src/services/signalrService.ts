import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5176/notificationHub';

export interface SignalRNotification {
    Key?: string;
    Args?: Record<string, unknown>;
    Message?: string;
}

type SignalRCallback = (data: string | SignalRNotification) => void;

export type ConnectionStatus = 'Connected' | 'Disconnected' | 'Connecting' | 'Reconnecting';

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private callbacks: Record<string, SignalRCallback[]> = {};
    private connectionStatus: ConnectionStatus = 'Disconnected';
    private onStatusChange: ((status: ConnectionStatus) => void) | null = null;

    public getStatus() {
        return this.connectionStatus;
    }

    public setStatusCallback(callback: (status: ConnectionStatus) => void) {
        this.onStatusChange = callback;
    }

    public async startConnection(accessTokenFactory?: () => string | Promise<string> | null) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) return;

        this.updateStatus('Connecting');

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: async () => {
                    if (accessTokenFactory) {
                        const token = await accessTokenFactory();
                        return token || '';
                    }
                    return '';
                }
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.onreconnecting(() => this.updateStatus('Reconnecting'));
        this.connection.onreconnected(() => this.updateStatus('Connected'));
        this.connection.onclose(() => this.updateStatus('Disconnected'));

        this.connection.on('ReceiveOrderNotification', (message: string) => {
            this.notifyListeners('ReceiveOrderNotification', message);
        });

        this.connection.on('ReceiveNotification', (data: string | SignalRNotification) => {
            this.notifyListeners('ReceiveNotification', data);
        });

        this.connection.on('ReceiveSystemNotification', (data: string | SignalRNotification) => {
            this.notifyListeners('ReceiveSystemNotification', data);
        });

        this.connection.on('ReceiveQuoteNotification', (message: string) => {
            this.notifyListeners('ReceiveQuoteNotification', message);
        });

        try {
            await this.connection.start();
            this.updateStatus('Connected');
            console.log('SignalR Connected');
        } catch (err) {
            this.updateStatus('Disconnected');
            console.error('SignalR Connection Error: ', err);
            // Retry after 5s if fails initially
            setTimeout(() => this.startConnection(), 5000);
        }
    }

    private updateStatus(status: ConnectionStatus) {
        this.connectionStatus = status;
        if (this.onStatusChange) this.onStatusChange(status);
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
