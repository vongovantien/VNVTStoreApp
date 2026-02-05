import { useState, useCallback } from 'react';
import { useMutation, useQueryClient, QueryKey } from '@tanstack/react-query';
import { ApiResponse } from '@/services/api';
import { useToast } from '@/store';
import { useTranslation } from 'react-i18next';

export interface EntityService<T, CreateDto, UpdateDto> {
    create: (data: CreateDto) => Promise<ApiResponse<unknown>>;
    update: (id: string | number, data: UpdateDto) => Promise<ApiResponse<unknown>>;
    delete: (id: string | number) => Promise<ApiResponse<unknown>>;
    getByCode?: (code: string, params?: Record<string, unknown>) => Promise<ApiResponse<T>>; // Optional fetch detail method
}

interface UseEntityManagerOptions<T, CreateDto, UpdateDto> {
    service: EntityService<T, CreateDto, UpdateDto>;
    queryKey: QueryKey;
    includeChildrenOnEdit?: boolean; // New option
    translations?: {
        createSuccess?: string;
        createError?: string;
        updateSuccess?: string;
        updateError?: string;
        deleteSuccess?: string;
        deleteError?: string;
    };
}

export const useEntityManager = <T extends { code?: string; id?: string | number }, CreateDto = Partial<T>, UpdateDto = Partial<T>>({
    service,
    queryKey,
    includeChildrenOnEdit = false,
    translations,
}: UseEntityManagerOptions<T, CreateDto, UpdateDto>) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const toast = useToast();

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingItem, setEditingItem] = useState<T | null>(null);
    const [itemToDelete, setItemToDelete] = useState<T | null>(null);
    const [isFetchingDetail, setIsFetchingDetail] = useState(false);

    const createMutation = useMutation({
        mutationFn: (data: CreateDto) => service.create(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey });
            toast.success(translations?.createSuccess || t('common.createSuccess'));
            setIsFormOpen(false);
        },
        onError: (error: Error) => {
            toast.error(error?.message || translations?.createError || t('common.createError'));
        },
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string | number; data: UpdateDto }) => service.update(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey });
            toast.success(translations?.updateSuccess || t('common.updateSuccess'));
            setIsFormOpen(false);
            setEditingItem(null);
        },
        onError: (error: Error) => {
            toast.error(error?.message || translations?.updateError || t('common.updateError'));
        },
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string | number) => service.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey });
            toast.success(translations?.deleteSuccess || t('common.deleteSuccess'));
            setItemToDelete(null);
        },
        onError: (error: Error) => {
            toast.error(error?.message || translations?.deleteError || t('common.deleteError'));
        },
    });

    const openCreate = useCallback(() => {
        setEditingItem(null);
        setIsFormOpen(true);
    }, []);

    const openEdit = useCallback(async (item: T) => {
        setEditingItem(item);
        setIsFormOpen(true);

        // Try to fetch fresh details if service supports it
        const id = item.code;
        if (service.getByCode && id) {
            setIsFetchingDetail(true);
            try {
                const response = await service.getByCode(id, includeChildrenOnEdit ? { includeChildren: true } : undefined);
                // Check if response wraps data in ApiResponse logic (success/data props)
                if (response && response.success && response.data) {
                    setEditingItem(response.data);
                } else if (response) {
                    // Fallback if response is direct object or different format
                    setEditingItem(response.data as T);
                }
            } catch (err) {
                console.error("Failed to fetch fresh item details", err);
                // Optionally toast warning?
            } finally {
                setIsFetchingDetail(false);
            }
        }
    }, [service, includeChildrenOnEdit]);

    const closeForm = useCallback(() => {
        setIsFormOpen(false);
        setEditingItem(null);
    }, []);

    const confirmDelete = useCallback((item: T) => {
        setItemToDelete(item);
    }, []);

    const cancelDelete = useCallback(() => {
        setItemToDelete(null);
    }, []);

    const processCreate = useCallback((data: CreateDto) => {
        createMutation.mutate(data);
    }, [createMutation]);

    const processUpdate = useCallback((id: string | number, data: UpdateDto) => {
        updateMutation.mutate({ id, data });
    }, [updateMutation]);

    const processDelete = useCallback((id: string | number) => {
        deleteMutation.mutate(id);
    }, [deleteMutation]);

    return {
        // State
        isFormOpen,
        editingItem,
        itemToDelete,
        isFetchingDetail,

        // Actions
        openCreate,
        openEdit,
        closeForm,
        confirmDelete,
        cancelDelete,

        // Processors
        create: processCreate,
        update: processUpdate,
        delete: processDelete,

        // Mutations (full objects if needed)
        createMutation,
        updateMutation,
        deleteMutation,

        // Loading states
        isLoading: createMutation.isPending || updateMutation.isPending || deleteMutation.isPending,
        isCreating: createMutation.isPending,
        isUpdating: updateMutation.isPending,
        isDeleting: deleteMutation.isPending,
    };
};
