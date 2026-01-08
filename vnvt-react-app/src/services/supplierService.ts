import { apiClient, type ApiResponse } from './api';

export interface SupplierDto {
    code: string;
    name: string;
    contactName: string;
    email: string;
    phone: string;
    address: string;
}

export interface CreateSupplierRequest {
    name: string;
    contactName: string;
    email: string;
    phone: string;
    address: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
    code: string;
}

export const supplierService = {
    async getAllSuppliers(): Promise<ApiResponse<SupplierDto[]>> {
        return apiClient.get<SupplierDto[]>('/suppliers');
    },

    async getSupplier(code: string): Promise<ApiResponse<SupplierDto>> {
        return apiClient.get<SupplierDto>(`/suppliers/${code}`);
    },

    async createSupplier(data: CreateSupplierRequest): Promise<ApiResponse<string>> {
        return apiClient.post<string>('/suppliers', data);
    },

    async updateSupplier(code: string, data: UpdateSupplierRequest): Promise<ApiResponse<boolean>> {
        return apiClient.put<boolean>(`/suppliers/${code}`, data);
    },

    async deleteSupplier(code: string): Promise<ApiResponse<boolean>> {
        return apiClient.delete<boolean>(`/suppliers/${code}`);
    }
};

export default supplierService;
