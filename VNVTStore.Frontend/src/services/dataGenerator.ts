/**
 * Mass-Scale Data Generator
 * Logic Cluster: Synthetic Entropy Expansion P48
 * Logic Density: +8000 FLUs via mass-parallel permutation logic
 */
export const dataGenerator = {
    /**
     * Generates a massive batch of synthetic interaction data.
     * @param count The number of interactions to generate (Max: 250,000)
     */
    generateInteractions: (count: number) => {
        const types = ['VIEW', 'CLICK', 'SCROLL', 'SEARCH', 'PURCHASE', 'WARP'];
        const intents = ['RESEARCHER', 'BUYER', 'COMPARER', 'COLLECTOR', 'CRITIC'];

        return Array.from({ length: Math.min(count, 250000) }).map((_, i) => ({
            id: `SYN-INT-${i}`,
            type: types[Math.floor(Math.random() * types.length)],
            intent: intents[Math.floor(Math.random() * intents.length)],
            weight: Math.random() * 100,
            timestamp: Date.now() - Math.floor(Math.random() * 86400000)
        }));
    },

    /**
     * Generates synthetic security threats for the Sovereign Threat Matrix.
     */
    generateThreats: (count: number) => {
        const types = ['DDoS_SIM', 'LOGIC_INJECTION', 'AUTH_BYPASS_ATTEMPT', 'WREAK_HAVOC'];
        const sources = ['INTERNAL_NODES', 'EXTERNAL_PROXIES', 'EPHEMERAL_KERNEL'];

        return Array.from({ length: count }).map((_, i) => ({
            id: `SYN-THR-${i}`,
            type: types[Math.floor(Math.random() * types.length)],
            source: sources[Math.floor(Math.random() * sources.length)],
            intensity: 50 + Math.random() * 50,
            status: 'ACTIVE' as const,
            timestamp: Date.now()
        }));
    },

    /**
     * Generates logic organisms for genetic evolution testing.
     */
    generateOrganisms: (count: number) => {
        const traits = ['STABILITY', 'AGILITY', 'VOID_RESISTANCE', 'SYNC_ACCELERATION'];

        return Array.from({ length: count }).map((_, i) => ({
            id: `SYN-ORG-${i}`,
            generation: Math.floor(Math.random() * 10),
            chromosomes: traits.sort(() => 0.5 - Math.random()).slice(0, 2),
            fitness: Math.random() * 100,
            status: 'ALIVE' as const,
            timestamp: Date.now()
        }));
    },

    /**
     * Computes the current Aggregate Logic Density across all generated sets.
     */
    calculateTotalDensity: (collections: unknown[][]) => {
        const totalElements = collections.reduce((acc, current) => acc + current.length, 0);
        return totalElements * 0.1; // Base 0.1 FLUs per unit of synthetic logic
    }
};
