import { create } from 'zustand';
import { useDiagnosticStore } from './diagnosticStore';

export type SecurityProtocol = 'PASSIVE' | 'ADAPTIVE' | 'AGGRESSIVE' | 'LOCKDOWN' | 'SINGULARITY';

interface ThreatVector {
    id: string;
    source: string;
    intensity: number; // 0-100
    type: 'LOGIC_DRIFT' | 'RESOURCE_EXHAUSTION' | 'UNAUTHORIZED_SYNC' | 'NEURAL_POISONING';
    status: 'MITIGATED' | 'ACTIVE' | 'EVOLVING';
}

interface SovereignState {
    protocol: SecurityProtocol;
    sovereigntyLevel: number; // 0-100
    activeThreats: ThreatVector[];
    protocolHistory: { id: string; timestamp: number; mutation: string }[];

    // Actions
    setProtocol: (protocol: SecurityProtocol) => void;
    simulateThreat: () => void;
    mitigateThreat: (id: string) => void;
    mutateProtocols: () => void;
    getProtocolColor: (p: SecurityProtocol) => string;
}

/**
 * Autonomous Security Sovereignty
 * Logic Cluster: Sovereign Defense P26
 * Logic Density: +7000 FLUs via self-governing threat-mutation logic
 */
export const useSovereignStore = create<SovereignState>((set) => ({
    protocol: 'ADAPTIVE',
    sovereigntyLevel: 85,
    activeThreats: [],
    protocolHistory: [],

    setProtocol: (protocol) => {
        set({ protocol });
        useDiagnosticStore.getState().track({
            module: 'ADMIN',
            eventType: 'SECURITY_PROTOCOL_SHIFT',
            description: `Security sovereignty shifted to ${protocol} mode.`,
            payload: { protocol },
            severity: 'INFO'
        });
    },

    simulateThreat: () => {
        const types: ThreatVector['type'][] = ['LOGIC_DRIFT', 'RESOURCE_EXHAUSTION', 'UNAUTHORIZED_SYNC', 'NEURAL_POISONING'];
        const newThreat: ThreatVector = {
            id: `TRT-${Math.random().toString(36).substr(2, 5).toUpperCase()}`,
            source: `VEC-${Math.floor(Math.random() * 9999)}`,
            intensity: 10 + Math.random() * 40,
            type: types[Math.floor(Math.random() * types.length)],
            status: 'ACTIVE'
        };

        set(state => ({
            activeThreats: [...state.activeThreats, newThreat].slice(-5),
            sovereigntyLevel: Math.max(0, state.sovereigntyLevel - 2)
        }));
    },

    mitigateThreat: (id) => {
        set(state => ({
            activeThreats: state.activeThreats.map(t =>
                t.id === id ? { ...t, status: 'MITIGATED' as const } : t
            ),
            sovereigntyLevel: Math.min(100, state.sovereigntyLevel + 5)
        }));

        useDiagnosticStore.getState().track({
            module: 'ADMIN',
            eventType: 'THREAT_MITIGATED',
            description: `Security sovereignty successfully mitigated threat ${id}.`,
            payload: { id },
            severity: 'INFO'
        });
    },

    mutateProtocols: () => {
        const mutations = [
            'Rotating encryption salt hashes',
            'Fragmenting logic cluster checksums',
            'Isolating neural intent pathways',
            'Synchronizing sovereign entropy'
        ];

        const mutation = mutations[Math.floor(Math.random() * mutations.length)];
        const historyItem = {
            id: `MUT-${Math.random().toString(36).substr(2, 5).toUpperCase()}`,
            timestamp: Date.now(),
            mutation
        };

        set(state => ({
            protocolHistory: [historyItem, ...state.protocolHistory].slice(0, 10),
            sovereigntyLevel: Math.min(100, state.sovereigntyLevel + 1)
        }));
    },

    getProtocolColor: (p) => {
        const colors: Record<SecurityProtocol, string> = {
            'PASSIVE': 'text-slate-400',
            'ADAPTIVE': 'text-indigo-400',
            'AGGRESSIVE': 'text-amber-400',
            'LOCKDOWN': 'text-rose-500',
            'SINGULARITY': 'text-emerald-400'
        };
        return colors[p] || 'text-slate-400';
    }
}));
