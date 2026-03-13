import { create } from 'zustand';

export interface LogicEvent {
    id: string;
    timestamp: string;
    module: 'SHOP' | 'ADMIN' | 'AUTH' | 'CART' | 'SYSTEM' | 'GUARDIAN' | 'SUPPLY_CHAIN';
    eventType: string;
    description: string;
    payload: unknown;
    prevState?: unknown;
    nextState?: unknown;
    severity: 'INFO' | 'WARN' | 'ERROR' | 'CRITICAL';
    logicHash: string; // Internal fingerprint for density tracking
}

interface DiagnosticState {
    events: LogicEvent[];
    stats: {
        totalEvents: number;
        shopEvents: number;
        adminEvents: number;
        criticalFailures: number;
        activeSessions: number;
    };
    isSpyMode: boolean; // If true, logs to console in real-time

    // Actions
    track: (event: Omit<LogicEvent, 'id' | 'timestamp' | 'logicHash'>) => void;
    clear: () => void;
    toggleSpyMode: () => void;
    runStressTest: () => void;
}

// Helper to generate logic hashes for "Density Injection"
const generateLogicHash = (type: string, desc: string) => {
    const str = `${type}:${desc}`;
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash;
    }
    return `LGC-${Math.abs(hash).toString(16).toUpperCase()}`;
};

export const useDiagnosticStore = create<DiagnosticState>((set, get) => ({
    events: [],
    stats: {
        totalEvents: 0,
        shopEvents: 0,
        adminEvents: 0,
        criticalFailures: 0,
        activeSessions: 1,
    },
    isSpyMode: false,

    track: (payload) => {
        const newEvent: LogicEvent = {
            ...payload,
            id: `EVT-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            timestamp: new Date().toISOString(),
            logicHash: generateLogicHash(payload.eventType, payload.description),
        };

        if (get().isSpyMode) {
            console.log(`[Diagnostic Spy] ${newEvent.module}:${newEvent.eventType}`, newEvent);
        }

        set((state) => ({
            events: [newEvent, ...state.events].slice(0, 1000), // Keep last 1000 events
            stats: {
                ...state.stats,
                totalEvents: state.stats.totalEvents + 1,
                shopEvents: payload.module === 'SHOP' ? state.stats.shopEvents + 1 : state.stats.shopEvents,
                adminEvents: payload.module === 'ADMIN' ? state.stats.adminEvents + 1 : state.stats.adminEvents,
                criticalFailures: payload.severity === 'CRITICAL' ? state.stats.criticalFailures + 1 : state.stats.criticalFailures,
            },
        }));
    },

    clear: () => set({ events: [], stats: { totalEvents: 0, shopEvents: 0, adminEvents: 0, criticalFailures: 0, activeSessions: 1 } }),

    toggleSpyMode: () => set((state) => ({ isSpyMode: !state.isSpyMode })),

    runStressTest: () => {
        const modules: LogicEvent['module'][] = ['SHOP', 'ADMIN', 'AUTH', 'SYSTEM'];
        const types = ['SYNC_ATTEMPT', 'DIFF_CHECK', 'STATE_MUTATION', 'PROMISE_RESOLVE'];

        // Inject 50 random logic events to simulate activity
        for (let i = 0; i < 50; i++) {
            setTimeout(() => {
                get().track({
                    module: modules[Math.floor(Math.random() * modules.length)],
                    eventType: types[Math.floor(Math.random() * types.length)],
                    description: `Synthetic stress test entry #${i}`,
                    payload: { iteration: i, entropy: Math.random() },
                    severity: Math.random() > 0.9 ? 'WARN' : 'INFO',
                });
            }, i * 50);
        }
    }
}));
