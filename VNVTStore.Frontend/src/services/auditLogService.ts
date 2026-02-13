import { createEntityService } from './baseService';
import { API_ENDPOINTS } from '@/constants';

export interface AuditLogDto {
    code: string;
    userCode?: string;
    userName?: string;
    action: string;
    target?: string;
    detail?: string;
    ipAddress?: string;
    createdAt: string;
    isActive: boolean;
}

// RequestDTO structure matches backend
export interface SearchParams {
    pageIndex: number;
    pageSize: number;
    searching?: any[];
    sortDTO?: {
        sortBy: string;
        sort: string;
    };
}

const baseService = createEntityService<AuditLogDto, any, any>({
    endpoint: API_ENDPOINTS.AUDIT_LOGS.BASE,
});

export const auditLogService = {
    ...baseService,
    // Base service already has 'search' which sends POST request
};

export default auditLogService;
