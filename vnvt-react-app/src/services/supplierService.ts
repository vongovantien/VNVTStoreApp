/**
 * Supplier Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

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
export const supplierService = createCrudService<SupplierDto, CreateSupplierRequest, UpdateSupplierRequest>({
    endpoint: API_ENDPOINTS.SUPPLIERS.BASE,
    resourceName: 'Supplier'
});

export default supplierService;
