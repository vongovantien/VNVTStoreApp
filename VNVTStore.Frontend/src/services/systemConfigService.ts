import { ApiResponse } from './api';
import apiClient from './api';

export interface SystemConfigDto {
    configKey: string;
    configValue: string;
    description?: string;
    isActive: boolean;
    updatedAt?: string;
}

export interface UpdateSystemConfigRequest {
    configKey: string;
    configValue: string;
    isActive?: boolean;
}

class SystemConfigService {
    endpoint = '/systemconfig';

    async get(key: string): Promise<ApiResponse<SystemConfigDto>> {
        return apiClient.get(`${this.endpoint}/${key}`);
    }

    async update(data: UpdateSystemConfigRequest): Promise<ApiResponse<SystemConfigDto>> {
        return apiClient.post(`${this.endpoint}`, data);
    }
}

export const systemConfigService = new SystemConfigService();
