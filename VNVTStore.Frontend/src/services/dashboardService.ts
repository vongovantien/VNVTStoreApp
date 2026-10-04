/**
 * Dashboard Service
 * Custom service for dashboard statistics & revenue reporting
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

export interface RevenueSummaryDto {
    totalRevenue: number;
    totalOrders: number;
    averageOrderValue: number;
    completedOrders: number;
    cancelledOrders: number;
    pendingOrders: number;
    totalDiscount: number;
    revenueChangeVsPreviousPeriod: number;
    ordersChangeVsPreviousPeriod: number;
}

export interface RevenueTimelineDto {
    date: string;
    label: string;
    revenue: number;
    totalOrders: number;
    completedOrders: number;
    cancelledOrders: number;
}

export interface PaymentMethodStatDto {
    method: string;
    revenue: number;
    orderCount: number;
    percentage: number;
}

export interface OrderStatusStatDto {
    status: string;
    count: number;
    totalAmount: number;
    percentage: number;
}

export interface RevenueReportDto {
    summary: RevenueSummaryDto;
    timeline: RevenueTimelineDto[];
    topProducts: { name: string; sales: number; revenue: number }[];
    paymentMethods: PaymentMethodStatDto[];
    orderStatuses: OrderStatusStatDto[];
}

// ============ Service ============
export const dashboardService = {
    async getStats(dateRange?: { startDate: string; endDate: string }): Promise<ApiResponse<DashboardStatsDto>> {
        const params = dateRange
            ? { startDate: dateRange.startDate, endDate: dateRange.endDate }
            : {};

        const response = await apiClient.get<Record<string, unknown>>(
            API_ENDPOINTS.DASHBOARD.STATS,
            { params }
        );

        if (response.success && response.data) {
            const data = response.data;
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
    },

    async getRevenueReport(params?: { startDate?: string; endDate?: string; groupBy?: string }): Promise<ApiResponse<RevenueReportDto>> {
        const queryParams = params
            ? { startDate: params.startDate, endDate: params.endDate, groupBy: params.groupBy }
            : {};

        const response = await apiClient.get<Record<string, unknown>>(
            API_ENDPOINTS.DASHBOARD.REVENUE_REPORT,
            { params: queryParams }
        );

        if (response.success && response.data) {
            const data = response.data as unknown as RevenueReportDto;
            return { ...response, data };
        }
        return response as unknown as ApiResponse<RevenueReportDto>;
    }
};

export default dashboardService;
