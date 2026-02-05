/**
 * Unit Service
 */
import { createEntityService, API_ENDPOINTS } from './baseService';

export interface CatalogUnitDto {
    code: string;
    name: string;
    isActive: boolean;
    // Optional: createdAt, updatedAt
}

export interface CreateUnitRequest {
    name: string;
    isActive?: boolean;
}

export type UpdateUnitRequest = Partial<CreateUnitRequest>;

export const unitService = createEntityService<CatalogUnitDto, CreateUnitRequest, UpdateUnitRequest>({
    endpoint: API_ENDPOINTS.UNITS?.BASE || '/units',
    resourceName: 'Unit'
});

export default unitService;
