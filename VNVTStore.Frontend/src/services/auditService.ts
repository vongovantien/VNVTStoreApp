import { apiClient, ApiResponse, PagedResult } from './api';

export interface AuditLog {
    id: string;
    action: string;
    module: string;
    user: string;
    details: string;
    ipAddress: string;
    timestamp: string;
    status: 'success' | 'failure';
}

class AuditService {
    async getLogs(params?: {
        pageIndex?: number;
        pageSize?: number;
        module?: string;
        action?: string;
        user?: string;
    }): Promise<ApiResponse<PagedResult<AuditLog>>> {
        return await apiClient.get<PagedResult<AuditLog>>('/audit-logs', { params });
    }
}

export const auditService = new AuditService();
