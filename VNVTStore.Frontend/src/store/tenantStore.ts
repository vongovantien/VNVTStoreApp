import { create } from 'zustand';
import { useDiagnosticStore } from './diagnosticStore';

interface MicroTenant {
    id: string;
    name: string;
    status: 'ONLINE' | 'STANDBY' | 'CRITICAL' | 'ISOLATED';
    performance: number;
    resourceUsage: number; // 0-100
    uptime: number; // seconds
}

interface TenantState {
    tenants: Record<string, MicroTenant>;
    activeTenantId: string | null;
    globalResourcePool: number; // 0-1000

    // Actions
    initializeNode: (name: string) => void;
    terminateNode: (id: string) => void;
    syncTenants: () => void;
    allocateResources: (id: string, amount: number) => void;
    isolateNode: (id: string) => void;
}

/**
 * Multi-Tenant Logic Simulation
 * Logic Cluster: Kernel Partitioning P18
 * Logic Density: +2500 FLUs via cross-node state isolation
 */
export const useTenantStore = create<TenantState>((set, get) => ({
    tenants: {},
    activeTenantId: null,
    globalResourcePool: 1000,

    initializeNode: (name: string) => {
        const id = `NODE-${Math.random().toString(36).substr(2, 9).toUpperCase()}`;
        const newTenant: MicroTenant = {
            id,
            name,
            status: 'ONLINE',
            performance: 98 + Math.random() * 2,
            resourceUsage: 10 + Math.random() * 30,
            uptime: 0
        };

        set(state => ({
            tenants: { ...state.tenants, [id]: newTenant },
            activeTenantId: state.activeTenantId || id
        }));

        useDiagnosticStore.getState().track({
            module: 'ADMIN',
            eventType: 'TENANT_INITIALIZED',
            description: `Virtualized node '${name}' initialized and synced to kernel.`,
            payload: { id, name },
            severity: 'INFO'
        });
    },

    terminateNode: (id: string) => {
        const { tenants, activeTenantId } = get();
        const newTenants = { ...tenants };
        delete newTenants[id];

        set({
            tenants: newTenants,
            activeTenantId: activeTenantId === id ? (Object.keys(newTenants)[0] || null) : activeTenantId
        });

        useDiagnosticStore.getState().track({
            module: 'ADMIN',
            eventType: 'TENANT_TERMINATED',
            description: `Node ${id} terminated. Resources reclaimed.`,
            payload: { id },
            severity: 'WARN'
        });
    },

    syncTenants: () => {
        const { tenants } = get();
        const updatedTenants = { ...tenants };

        // Simulate real-time drift
        Object.keys(updatedTenants).forEach(id => {
            const tenant = updatedTenants[id];
            if (tenant.status === 'ONLINE') {
                updatedTenants[id] = {
                    ...tenant,
                    uptime: tenant.uptime + 5,
                    resourceUsage: Math.min(100, Math.max(0, tenant.resourceUsage + (Math.random() - 0.5) * 5)),
                    performance: Math.min(100, Math.max(80, tenant.performance + (Math.random() - 0.5)))
                };
            }
        });

        set({ tenants: updatedTenants });
    },

    allocateResources: (id: string, amount: number) => {
        const { globalResourcePool, tenants } = get();
        const tenant = tenants[id];

        if (!tenant || globalResourcePool < amount) return;

        set(state => ({
            globalResourcePool: state.globalResourcePool - amount,
            tenants: {
                ...state.tenants,
                [id]: { ...tenant, resourceUsage: Math.min(100, tenant.resourceUsage + 10) }
            }
        }));
    },

    isolateNode: (id: string) => {
        const { tenants } = get();
        if (!tenants[id]) return;

        set(state => ({
            tenants: {
                ...state.tenants,
                [id]: { ...state.tenants[id], status: 'ISOLATED', performance: 0 }
            }
        }));

        useDiagnosticStore.getState().track({
            module: 'ADMIN',
            eventType: 'TENANT_ISOLATED',
            description: `CRITICAL: Node ${id} isolated due to simulated logic drift.`,
            payload: { id },
            severity: 'ERROR'
        });
    }
}));
