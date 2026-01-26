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
    topProducts?: { name: string; sales: number; revenue: number }[];
    revenueChart?: { label: string; revenue: number; orderCount: number }[];
}

// ============ Service ============
// ============ Service ============
export const dashboardService = {
    async getStats(): Promise<ApiResponse<DashboardStatsDto>> {
        const response = await apiClient.get<Record<string, unknown>>(API_ENDPOINTS.DASHBOARD.STATS);

        console.log('Dashboard Stats Raw Response:', response);

        if (response.success && response.data) {
            const data = response.data;
            console.log('Dashboard Stats Data:', data);

            const mappedData: DashboardStatsDto = {
                totalRevenue: Number(data.totalRevenue ?? data.TotalRevenue ?? 0),
                totalOrders: Number(data.totalOrders ?? data.TotalOrders ?? 0),
                totalProducts: Number(data.totalProducts ?? data.TotalProducts ?? 0),
                totalCustomers: Number(data.totalCustomers ?? data.TotalCustomers ?? 0),
                revenueChange: Number(data.revenueChange ?? data.RevenueChange ?? 0),
                ordersChange: Number(data.ordersChange ?? data.OrdersChange ?? 0),
                customersChange: Number(data.customersChange ?? data.CustomersChange ?? 0),
                pendingQuotes: Number(data.pendingQuotes ?? data.PendingQuotes ?? 0),
                topProducts: (Array.isArray(data.topProducts || data.TopProducts) ? (data.topProducts || data.TopProducts) as Record<string, unknown>[] : []).map(p => ({
                    name: String(p.name ?? p.Name ?? ''),
                    sales: Number(p.sales ?? p.Sales ?? 0),
                    revenue: Number(p.revenue ?? p.Revenue ?? 0)
                })),
                revenueChart: (Array.isArray(data.revenueChart || data.RevenueChart) ? (data.revenueChart || data.RevenueChart) as Record<string, unknown>[] : []).map(c => ({
                    label: String(c.label ?? c.Label ?? ''),
                    revenue: Number(c.revenue ?? c.Revenue ?? 0),
                    orderCount: Number(c.orderCount ?? c.OrderCount ?? 0)
                })),
            };
            return { ...response, data: mappedData };
        }
        return response as unknown as ApiResponse<DashboardStatsDto>;
    }
};

export default dashboardService;
