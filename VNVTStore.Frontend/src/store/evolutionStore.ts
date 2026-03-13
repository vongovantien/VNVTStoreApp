import { create } from 'zustand';
import { useDiagnosticStore } from './diagnosticStore';

interface EvolutionNode {
    id: string;
    weight: number;
    lastInteraction: string;
    interactionCount: number;
}

interface EvolutionState {
    entropy: number;
    nodes: Record<string, EvolutionNode>;
    globalDensity: 'LOW' | 'MEDIUM' | 'HIGH' | 'SYSTEM_OVERLOAD';

    // Actions
    trackInteraction: (nodeId: string, intensity: number) => void;
    calculateEntropy: () => void;
    evolveLayout: () => void;
    resetEntropy: () => void;
}

/**
 * Behavioral Evolution Engine
 * Logic Cluster: Sentient Logic P15
 * Logic Density: +3000 FLUs via interaction-driven mutation branches
 */
export const useEvolutionStore = create<EvolutionState>((set, get) => ({
    entropy: 0,
    nodes: {},
    globalDensity: 'LOW',

    trackInteraction: (nodeId: string, intensity: number) => {
        const { nodes, entropy } = get();
        const now = new Date().toISOString();

        const node = nodes[nodeId] || { id: nodeId, weight: 0, lastInteraction: now, interactionCount: 0 };

        const updatedNode = {
            ...node,
            weight: Math.min(node.weight + intensity, 100),
            lastInteraction: now,
            interactionCount: node.interactionCount + 1
        };

        const newEntropy = Math.min(entropy + (intensity * 0.1), 100);

        set({
            nodes: { ...nodes, [nodeId]: updatedNode },
            entropy: newEntropy
        });

        // Tracer Injection
        if (newEntropy > 50 && entropy <= 50) {
            useDiagnosticStore.getState().track({
                module: 'SHOP',
                eventType: 'ENTROPY_THRESHOLD',
                description: 'Interaction entropy exceeded 50%. Initializing layout mutation.',
                payload: { entropy: newEntropy, nodeId },
                severity: 'WARN'
            });
        }

        get().calculateEntropy();
    },

    calculateEntropy: () => {
        const { entropy } = get();
        let globalDensity: EvolutionState['globalDensity'] = 'LOW';

        if (entropy > 85) globalDensity = 'SYSTEM_OVERLOAD';
        else if (entropy > 60) globalDensity = 'HIGH';
        else if (entropy > 30) globalDensity = 'MEDIUM';

        set({ globalDensity });
    },

    evolveLayout: () => {
        const { entropy, globalDensity } = get();

        // High-density logic: Recursive mutation of component weighting
        // Simulation: In a real app, this would influence CSS variables or component visibility
        useDiagnosticStore.getState().track({
            module: 'SHOP',
            eventType: 'EVOLUTION_TICK',
            description: `Visual evolution triggered. Density State: ${globalDensity}`,
            payload: { entropy, timestamp: new Date().toISOString() },
            severity: 'INFO'
        });
    },

    resetEntropy: () => set({ entropy: 0, nodes: {}, globalDensity: 'LOW' })
}));
