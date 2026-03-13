import { useServiceContext, AppServices } from './ServiceContext';

// Generic hook to get a service by key
export function useService<T>(serviceKey: keyof AppServices): T {
    const services = useServiceContext();
    const service = services[serviceKey];

    if (!service) {
        throw new Error(`Service '${String(serviceKey)}' not found in ServiceProvider`);
    }

    return service as T;
}
