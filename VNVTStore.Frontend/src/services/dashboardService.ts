/**
 * Dashboard Service
 * Custom service for dashboard statistics
 */

import { apiClient } from './api';
import { API_ENDPOINTS, type ApiResponse } from './baseService';

// ============ Types ============
export interface DashboardStatsDto {
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
export const dashboardService = {
    async getStats(): Promise<ApiResponse<DashboardStatsDto>> {
        return apiClient.get<DashboardStatsDto>(API_ENDPOINTS.DASHBOARD.STATS);
    }
};

export default dashboardService;
