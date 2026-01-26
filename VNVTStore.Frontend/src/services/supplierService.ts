/**
 * Supplier Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';
import apiClient from './api';

// ============ Types ============
export interface SupplierDto {
    code: string;
    name: string;
    contactPerson?: string;
    email?: string;
    phone?: string;
    address?: string;
    taxCode?: string;
    bankAccount?: string;
    bankName?: string;
    notes?: string;
    isActive: boolean;
    createdAt?: string;
    updatedAt?: string;
}

export interface CreateSupplierRequest {
    name: string;
    contactPerson?: string;
    email?: string;
    phone?: string;
    address?: string;
    taxCode?: string;
    bankAccount?: string;
    bankName?: string;
    notes?: string;
}

export interface UpdateSupplierRequest extends Partial<CreateSupplierRequest> {
    isActive?: boolean;
}

// ============ Service ============
export interface SupplierStats {
    total: number;
    active: number;
}

export const supplierService = {
    ...createEntityService<SupplierDto, CreateSupplierRequest, UpdateSupplierRequest>({
        endpoint: API_ENDPOINTS.SUPPLIERS.BASE,
    }),
    getStats: async (): Promise<SupplierStats> => {
        const response = await apiClient.get<SupplierStats>(`${API_ENDPOINTS.SUPPLIERS.BASE}/stats`);
        return response.data || { total: 0, active: 0 };
    }
};

export default supplierService;
