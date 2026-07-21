import { ApiResponse } from './api';
import apiClient from './api';

export interface SystemConfigDto extends Record<string, unknown> {
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

    async getAll(): Promise<ApiResponse<SystemConfigDto[]>> {
        return apiClient.get(this.endpoint);
    }

    async getPublicContacts(): Promise<ApiResponse<Record<string, string>>> {
        return apiClient.get('/configs/contacts');
    }

    async get(key: string): Promise<ApiResponse<SystemConfigDto>> {
        return apiClient.get(`${this.endpoint}/${key}`);
    }

    async update(data: UpdateSystemConfigRequest): Promise<ApiResponse<SystemConfigDto>> {
        return apiClient.post(`${this.endpoint}`, data);
    }

    async export(): Promise<void> {
        const response = await apiClient.get(`${this.endpoint}/export`, { responseType: 'blob' });
        const url = window.URL.createObjectURL(new Blob([response.data as BlobPart]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `SystemConfigs_${new Date().toISOString().slice(0, 10)}.xlsx`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    }

    async import(file: File): Promise<ApiResponse<number>> {
        const formData = new FormData();
        formData.append('file', file);
        return apiClient.post(`${this.endpoint}/import`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
    }
}

export const systemConfigService = new SystemConfigService();
