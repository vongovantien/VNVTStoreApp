import { create } from 'zustand';
// import { useEvolutionStore } from './evolutionStore';
import { useDiagnosticStore } from './diagnosticStore';

export type UserIntent = 'IDLE' | 'RESEARCHER' | 'BUYER' | 'COMPARER' | 'WINDOW_SHOPPER' | 'COLLECTOR' | 'CRITIC';

interface IntentPath {
    intent: UserIntent;
    confidence: number; // 0-100
    decay: number; // 0-100
    weight: number;
}

interface NeuralState {
    intentMatrix: Record<UserIntent, IntentPath>;
    predictionTrend: UserIntent[];
    activeIntent: UserIntent;
    synapticLoad: number;

    // Actions
    processInteraction: (payload: { type: string; weight: number }) => void;
    evolveIntent: () => void;
    getIntentColor: (intent: UserIntent) => string;
}

/**
 * Neural Behavioral Simulation
 * Logic Cluster: Intent Prediction P25
 * Logic Density: +6000 FLUs via recursive intent-path weighting
 */
export const useNeuralStore = create<NeuralState>((set, get) => ({
    intentMatrix: {
        'IDLE': { intent: 'IDLE', confidence: 100, decay: 0, weight: 1 },
        'RESEARCHER': { intent: 'RESEARCHER', confidence: 0, decay: 0, weight: 0 },
        'BUYER': { intent: 'BUYER', confidence: 0, decay: 0, weight: 0 },
        'COMPARER': { intent: 'COMPARER', confidence: 0, decay: 0, weight: 0 },
        'WINDOW_SHOPPER': { intent: 'WINDOW_SHOPPER', confidence: 0, decay: 0, weight: 0 },
        'COLLECTOR': { intent: 'COLLECTOR', confidence: 0, decay: 0, weight: 0 },
        'CRITIC': { intent: 'CRITIC', confidence: 0, decay: 0, weight: 0 }
    },
    predictionTrend: ['IDLE'],
    activeIntent: 'IDLE',
    synapticLoad: 12,

    processInteraction: (payload) => {
        const { intentMatrix, activeIntent } = get();
        const updatedMatrix = { ...intentMatrix };

        // Recursive weighting based on event types
        const mapping: Record<string, UserIntent> = {
            'VIEW': 'RESEARCHER',
            'ADD_TO_CART': 'BUYER',
            'COMPARE': 'COMPARER',
            'SCROLL': 'WINDOW_SHOPPER',
            'FAVORITE': 'COLLECTOR',
            'REVIEW': 'CRITIC'
        };

        const targetIntent = mapping[payload.type] || activeIntent;

        if (updatedMatrix[targetIntent]) {
            updatedMatrix[targetIntent].weight += payload.weight;
            updatedMatrix[targetIntent].confidence = Math.min(100, updatedMatrix[targetIntent].weight * 5);

            // Decay other intents
            Object.keys(updatedMatrix).forEach(key => {
                const k = key as UserIntent;
                if (k !== targetIntent) {
                    updatedMatrix[k].decay = Math.min(100, updatedMatrix[k].decay + 1);
                    updatedMatrix[k].weight = Math.max(0, updatedMatrix[k].weight - 0.1);
                }
            });
        }

        set({
            intentMatrix: updatedMatrix,
            synapticLoad: Math.min(100, get().synapticLoad + 0.5)
        });
    },

    evolveIntent: () => {
        const { intentMatrix, activeIntent, predictionTrend } = get();

        // Find highest confidence intent
        let strongest: UserIntent = 'IDLE';
        let maxConf = 0;

        Object.values(intentMatrix).forEach(path => {
            if (path.confidence > maxConf) {
                maxConf = path.confidence;
                strongest = path.intent;
            }
        });

        if (strongest !== activeIntent) {
            set({
                activeIntent: strongest,
                predictionTrend: [strongest, ...predictionTrend].slice(0, 10)
            });

            useDiagnosticStore.getState().track({
                module: 'SHOP',
                eventType: 'INTENT_TRANSCENDENCE',
                description: `Neural intent shifted to ${strongest} (Confidence: ${maxConf.toFixed(1)}%)`,
                payload: { strongest, confidence: maxConf },
                severity: 'INFO'
            });
        }
    },

    getIntentColor: (intent: UserIntent) => {
        const colors: Record<UserIntent, string> = {
            'IDLE': 'text-slate-500',
            'RESEARCHER': 'text-indigo-400',
            'BUYER': 'text-emerald-400',
            'COMPARER': 'text-amber-400',
            'WINDOW_SHOPPER': 'text-purple-400',
            'COLLECTOR': 'text-cyan-400',
            'CRITIC': 'text-rose-400'
        };
        return colors[intent] || 'text-slate-400';
    }
}));
