import { categoryService, supplierService, unitService, brandService } from '@/services';
import { SearchCondition } from '@/services/baseService';

export const useProductFormOptions = (t: (key: string) => string) => {
    const fetchCategories = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
        const res = await categoryService.search({
            pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
            searchField: 'Name', sortBy: 'Code',
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            fields: ['Code', 'Name']
        });
        return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
    };

    const fetchSuppliers = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
        const res = await supplierService.search({
            pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
            searchField: 'Name', sortBy: 'Code',
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            fields: ['Code', 'Name']
        });
        return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
    };

    const fetchUnits = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
        const res = await unitService.search({
            pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
            searchField: 'Name', sortBy: 'Code',
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            fields: ['Code', 'Name']
        });
        return { items: (res.data?.items || []).map(x => ({ value: x.name, label: x.name })), totalItems: res.data?.totalItems || 0 };
    };

    const fetchBrands = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
        const res = await brandService.search({
            pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
            searchField: 'Name', sortBy: 'Code',
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            fields: ['Code', 'Name']
        });
        return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
    };

    return {
        fetchCategories,
        fetchSuppliers,
        fetchUnits,
        fetchBrands
    };
};
