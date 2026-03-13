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

// Internal interfaces for type safety
interface SearchCriterion {
    searchField: string;
    searchValue: string;
    searchCondition: string;
}

interface BackendAuditLog {
    code: string;
    action: string;
    target?: string;
    userName?: string;
    detail?: string;
    ipAddress?: string;
    createdAt: string;
    isActive: boolean;
}

class AuditService {
    async getLogs(params?: {
        pageIndex?: number;
        pageSize?: number;
        module?: string;
        action?: string;
        user?: string;
    }): Promise<ApiResponse<PagedResult<AuditLog>>> {
        const searchBody = {
            pageIndex: params?.pageIndex || 1,
            pageSize: params?.pageSize || 20,
            searching: [] as SearchCriterion[],
            sortDTO: {
                sortBy: "CreatedAt",
                sortDescending: true
            }
        };

        if (params?.module) {
            searchBody.searching.push({ searchField: "Target", searchValue: params.module, searchCondition: "Contains" });
        }
        if (params?.action) {
            searchBody.searching.push({ searchField: "Action", searchValue: params.action, searchCondition: "Contains" });
        }
        if (params?.user) {
            searchBody.searching.push({ searchField: "UserName", searchValue: params.user, searchCondition: "Contains" });
        }

        const response = await apiClient.post<PagedResult<BackendAuditLog>>('/audit-logs/search', searchBody);

        // Map backend DTO to frontend model
        if (response.data && response.data.items) {
            const mappedItems = response.data.items.map((item: BackendAuditLog) => ({
                id: item.code,
                action: item.action,
                module: item.target || 'N/A',
                user: item.userName || 'System',
                details: item.detail || '',
                ipAddress: item.ipAddress || '',
                timestamp: item.createdAt,
                status: (item.isActive ? 'success' : 'failure') as AuditLog['status'] // Mapping Valid/Invalid to Status
            }));

            return {
                ...response,
                data: {
                    ...response.data,
                    items: mappedItems
                }
            };
        }

        return {
            ...response,
            data: {
                items: [],
                totalItems: 0,
                pageIndex: 1,
                pageSize: 20,
                totalPages: 0,
                hasPreviousPage: false,
                hasNextPage: false
            }
        };
    }
}

export const auditService = new AuditService();
