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
export const dashboardService = {
    async getStats(): Promise<ApiResponse<DashboardStatsDto>> {
        const response = await apiClient.get<any>(API_ENDPOINTS.DASHBOARD.STATS);

        console.log('Dashboard Stats Raw Response:', response);

        // Manual mapping to handle potential PascalCase from backend
        if (response.success && response.data) {
            const data = response.data;
            // Handle case where data might be nested or have different structure
            console.log('Dashboard Stats Data:', data);

            const mappedData: DashboardStatsDto = {
                totalRevenue: data.totalRevenue ?? data.TotalRevenue ?? 0,
                totalOrders: data.totalOrders ?? data.TotalOrders ?? 0,
                totalProducts: data.totalProducts ?? data.TotalProducts ?? 0,
                totalCustomers: data.totalCustomers ?? data.TotalCustomers ?? 0,
                revenueChange: data.revenueChange ?? data.RevenueChange ?? 0,
                ordersChange: data.ordersChange ?? data.OrdersChange ?? 0,
                customersChange: data.customersChange ?? data.CustomersChange ?? 0,
                pendingQuotes: data.pendingQuotes ?? data.PendingQuotes ?? 0,
                topProducts: (data.topProducts ?? data.TopProducts ?? []).map((p: any) => ({
                    name: p.name ?? p.Name,
                    sales: p.sales ?? p.Sales,
                    revenue: p.revenue ?? p.Revenue
                })),
                revenueChart: (data.revenueChart ?? data.RevenueChart ?? []).map((c: any) => ({
                    label: c.label ?? c.Label,
                    revenue: c.revenue ?? c.Revenue,
                    orderCount: c.orderCount ?? c.OrderCount
                })),
            };
            return { ...response, data: mappedData };
        }
        return response as ApiResponse<DashboardStatsDto>;
    }
};

export default dashboardService;
