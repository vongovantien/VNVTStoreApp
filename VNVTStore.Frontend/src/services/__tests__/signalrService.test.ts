import { describe, it, expect, vi, beforeEach } from 'vitest';

// Fix for import error: mock before import
vi.mock('@microsoft/signalr', () => {
    const HubConnection = {
        start: vi.fn().mockResolvedValue(undefined),
        on: vi.fn(),
        off: vi.fn(),
        onreconnecting: vi.fn(),
        onreconnected: vi.fn(),
        onclose: vi.fn(),
        state: 'Disconnected'
    };

    const HubConnectionBuilder = function (this: {
        withUrl: () => unknown;
        withAutomaticReconnect: () => unknown;
        configureLogging: () => unknown;
        build: () => unknown;
    }) {
        this.withUrl = vi.fn().mockReturnThis();
        this.withAutomaticReconnect = vi.fn().mockReturnThis();
        this.configureLogging = vi.fn().mockReturnThis();
        this.build = vi.fn().mockReturnValue(HubConnection);
    };

    return {
        HubConnectionBuilder: HubConnectionBuilder,
        HubConnectionState: {
            Connected: 'Connected',
            Disconnected: 'Disconnected'
        },
        LogLevel: {
            Information: 1
        }
    };
});

import { signalRService } from '../signalrService';

describe('signalRService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('should start connection correctly', async () => {
        await signalRService.startConnection();
        expect(signalRService.getStatus()).toBe('Connected');
    });

    it('should register and notify listeners', () => {
        const callback = vi.fn();
        signalRService.on('test-event', callback);

        // Access private method for testing purpose
        interface ServiceWithPrivate { notifyListeners(v1: string, v2: string): void; }
        (signalRService as unknown as ServiceWithPrivate).notifyListeners('test-event', 'hello');

        expect(callback).toHaveBeenCalledWith('hello');
    });

    it('should unregister listeners', () => {
        const callback = vi.fn();
        signalRService.on('test-event', callback);
        signalRService.off('test-event', callback);

        interface ServiceWithPrivate { notifyListeners(v1: string, v2: string): void; }
        (signalRService as unknown as ServiceWithPrivate).notifyListeners('test-event', 'hello');

        expect(callback).not.toHaveBeenCalled();
    });
});
