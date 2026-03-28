import { ApiResponse } from './api';
import apiClient from './api';

export interface SystemSecretDto extends Record<string, unknown> {
    code: string;
    secretValue: string;
    description?: string;
    isActive: boolean;
    isEncrypted: boolean;
    updatedAt?: string;
}

export interface UpdateSystemSecretRequest {
    key: string;
    value: string;
    description?: string | null;
}

class SystemSecretService {
    endpoint = '/systemsecrets';

    async getAll(): Promise<ApiResponse<SystemSecretDto[]>> {
        return apiClient.get(this.endpoint);
    }

    async update(data: UpdateSystemSecretRequest): Promise<ApiResponse<boolean>> {
        return apiClient.post(this.endpoint, data);
    }

    async delete(key: string): Promise<ApiResponse<boolean>> {
        return apiClient.delete(`${this.endpoint}/${key}`);
    }

    async export(): Promise<void> {
        const response = await apiClient.get(`${this.endpoint}/export`, { responseType: 'blob' });
        const url = window.URL.createObjectURL(new Blob([response.data as BlobPart]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `SystemSecrets_${new Date().toISOString().slice(0, 10)}.xlsx`);
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

export const systemSecretService = new SystemSecretService();
