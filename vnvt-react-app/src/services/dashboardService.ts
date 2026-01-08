import { apiClient, type ApiResponse } from './api';

export interface DashboardStatsDto {
    totalRevenue: number;
    totalOrders: number;
    totalProducts: number;
    totalCustomers: number;
}

export const dashboardService = {
    async getStats(): Promise<ApiResponse<DashboardStatsDto>> {
        return apiClient.get<DashboardStatsDto>('/dashboard/stats');
    }
};

export default dashboardService;
