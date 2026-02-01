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
    return {
        HubConnectionBuilder: vi.fn().mockImplementation(() => ({
            withUrl: vi.fn().mockReturnThis(),
            withAutomaticReconnect: vi.fn().mockReturnThis(),
            configureLogging: vi.fn().mockReturnThis(),
            build: vi.fn().mockReturnValue(HubConnection)
        })),
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

        // Access private method for testing purpose via any
        (signalRService as any).notifyListeners('test-event', 'hello');

        expect(callback).toHaveBeenCalledWith('hello');
    });

    it('should unregister listeners', () => {
        const callback = vi.fn();
        signalRService.on('test-event', callback);
        signalRService.off('test-event', callback);

        (signalRService as any).notifyListeners('test-event', 'hello');

        expect(callback).not.toHaveBeenCalled();
    });
});
