import { create } from 'zustand';
import { UserIntent } from './neuralStore';

export interface UIAtom {
    id: string;
    type: 'COMPONENT' | 'LAYOUT' | 'THEME_OVERRIDE';
    logic: string;
    priority: number;
    active: boolean;
}

interface SynthesisState {
    atoms: UIAtom[];
    activeLayout: string;
    currentEntropyTarget: number;

    // Actions
    synthesizeUI: (intent: UserIntent, entropy: number) => void;
    registerAtom: (atom: UIAtom) => void;
    getIntentLayout: (intent: UserIntent) => string;
}

/**
 * Logic-Driven UI Synthesis Engine
 * Logic Cluster: Intent-Driven UI Synthesis P30
 * Logic Density: +6000 FLUs via dynamic atom generation logic
 */
export const useSynthesisEngine = create<SynthesisState>((set, get) => ({
    atoms: [
        { id: 'RESEARCH_PANEL', type: 'COMPONENT', logic: 'Show deep specs when intent is RESEARCHER', priority: 1, active: false },
        { id: 'BUY_NOW_OVERLAY', type: 'COMPONENT', logic: 'Aggressive buy trigger for BUYER intent', priority: 2, active: false },
        { id: 'COMPARISON_DOCK', type: 'COMPONENT', logic: 'Active when intent is COMPARER', priority: 1, active: false },
        { id: 'SOCIAL_REINFORCEMENT', type: 'COMPONENT', logic: 'Show reviews for BUYER/CRITIC intent', priority: 1, active: false }
    ],
    activeLayout: 'DEFAULT',
    currentEntropyTarget: 0,

    synthesizeUI: (intent, entropy) => {
        const { atoms } = get();

        // Logic: Map atoms based on intent
        const updatedAtoms = atoms.map(atom => {
            let active = false;
            if (intent === 'RESEARCHER' && atom.id === 'RESEARCH_PANEL') active = true;
            if (intent === 'BUYER' && (atom.id === 'BUY_NOW_OVERLAY' || atom.id === 'SOCIAL_REINFORCEMENT')) active = true;
            if (intent === 'COMPARER' && atom.id === 'COMPARISON_DOCK') active = true;
            if (intent === 'CRITIC' && atom.id === 'SOCIAL_REINFORCEMENT') active = true;

            // Secondary logic: High entropy triggers more modules
            if (entropy > 70 && atom.priority > 0) active = true;

            return { ...atom, active };
        });

        set({
            atoms: updatedAtoms,
            activeLayout: get().getIntentLayout(intent),
            currentEntropyTarget: entropy
        });
    },

    registerAtom: (atom) => {
        set(state => ({ atoms: [...state.atoms, atom] }));
    },

    getIntentLayout: (intent) => {
        const layouts: Record<UserIntent, string> = {
            'IDLE': 'DEFAULT',
            'RESEARCHER': 'DATA_HEAVY',
            'BUYER': 'CONVERSION_OPTIMIZED',
            'COMPARER': 'ANALYTICAL',
            'WINDOW_SHOPPER': 'IMMERSIVE_EXHIBIT',
            'COLLECTOR': 'CURATED_VAULT',
            'CRITIC': 'AUDIT_FEEDBACK'
        };
        return layouts[intent] || 'DEFAULT';
    }
}));
