/**
 * Dashboard Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface DashboardStatsDto {
    code: string;
    totalRevenue: number;
    totalOrders: number;
    totalProducts: number;
    totalCustomers: number;
    revenueChange?: number;
    ordersChange?: number;
    customersChange?: number;
    pendingQuotes?: number;
}

// ============ Service ============
export const dashboardService = createCrudService<DashboardStatsDto>({
    endpoint: API_ENDPOINTS.DASHBOARD.STATS,
    resourceName: 'Dashboard'
});

export default dashboardService;
