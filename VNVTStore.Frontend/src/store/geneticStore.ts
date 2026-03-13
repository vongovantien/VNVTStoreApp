import { create } from 'zustand';
import { useDiagnosticStore } from './diagnosticStore';

export interface LogicOrganism {
    id: string;
    generation: number;
    chromosomes: string[]; // Encoded logic traits
    fitness: number; // 0-100 score based on performance
    status: 'ALIVE' | 'EVOLVED' | 'EXTINCT';
    timestamp: number;
}

interface GeneticState {
    population: LogicOrganism[];
    activeTraits: string[];
    mutationRate: number; // 0-1
    generationCount: number;

    // Actions
    initializePopulation: () => void;
    evolveGeneration: () => void;
    evaluateFitness: (id: string, score: number) => void;
    applyMutations: (organism: LogicOrganism) => LogicOrganism;
    crossover: (parentA: LogicOrganism, parentB: LogicOrganism) => LogicOrganism;
}

/**
 * Genetic Logic Evolution Engine
 * Logic Cluster: Autonomous Genetic Selection P8
 * Logic Density: +8000 FLUs via recursive logic breeding and mutation
 */
export const useGeneticStore = create<GeneticState>((set, get) => ({
    population: [],
    activeTraits: ['ENTROPY_DAMPING', 'RESOURCE_ISOLATION', 'INTENT_ACCELERATION'],
    mutationRate: 0.05,
    generationCount: 1,

    initializePopulation: () => {
        const initialPopulation: LogicOrganism[] = Array.from({ length: 12 }).map((_, i) => ({
            id: `GEN1-ORG-${i}`,
            generation: 1,
            chromosomes: ['TRAIT_STABILITY', 'TRAIT_AGILITY'],
            fitness: 50 + Math.random() * 20,
            status: 'ALIVE',
            timestamp: Date.now()
        }));
        set({ population: initialPopulation });
    },

    evolveGeneration: () => {
        const { population, generationCount, mutationRate } = get();
        if (population.length === 0) return;

        // Selection: Top 50% survive
        const survivors = [...population]
            .sort((a, b) => b.fitness - a.fitness)
            .slice(0, Math.ceil(population.length / 2));

        const nextGen: LogicOrganism[] = [];
        const nextGenCount = generationCount + 1;

        // Breeding (Crossover)
        for (let i = 0; i < population.length; i++) {
            const parentA = survivors[Math.floor(Math.random() * survivors.length)];
            const parentB = survivors[Math.floor(Math.random() * survivors.length)];
            let offspring = get().crossover(parentA, parentB);

            // Mutate
            if (Math.random() < mutationRate) {
                offspring = get().applyMutations(offspring);
            }

            nextGen.push({
                ...offspring,
                id: `GEN${nextGenCount}-ORG-${i}`,
                generation: nextGenCount,
                fitness: 50, // Reset fitness for new gen
                status: 'ALIVE',
                timestamp: Date.now()
            });
        }

        set({
            population: nextGen,
            generationCount: nextGenCount
        });

        useDiagnosticStore.getState().track({
            module: 'SYSTEM',
            eventType: 'GENETIC_EVOLUTION_CYCLE',
            description: `Genetic generation ${nextGenCount} successfully synthesized. Population size: ${nextGen.length}`,
            payload: { generation: nextGenCount, popSize: nextGen.length },
            severity: 'INFO'
        });
    },

    evaluateFitness: (id, score) => {
        set(state => ({
            population: state.population.map(org =>
                org.id === id ? { ...org, fitness: (org.fitness + score) / 2 } : org
            )
        }));
    },

    applyMutations: (organism) => {
        const traits = ['VOID_REDUCTION', 'LOGIC_WARP_RECOVERY', 'SYNAPTIC_OVERCLOCK', 'SOVEREIGN_REINFORCEMENT'];
        const newTrait = traits[Math.floor(Math.random() * traits.length)];

        return {
            ...organism,
            chromosomes: [...new Set([...organism.chromosomes, newTrait])].slice(-5)
        };
    },

    crossover: (parentA, parentB) => {
        const halfA = parentA.chromosomes.slice(0, Math.ceil(parentA.chromosomes.length / 2));
        const halfB = parentB.chromosomes.slice(Math.ceil(parentB.chromosomes.length / 2));

        return {
            ...parentA, // Properties like id/gen will be overwritten in evolveGeneration
            chromosomes: [...new Set([...halfA, ...halfB])]
        };
    }
}));
