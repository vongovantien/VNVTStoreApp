import { useState } from 'react';
import { useMutation, useQueryClient, QueryKey } from '@tanstack/react-query';
import { useToast } from '@/store';
import { useTranslation } from 'react-i18next';

interface EntityService<T, CreateDto, UpdateDto> {
    create: (data: CreateDto) => Promise<any>;
    update: (id: any, data: UpdateDto) => Promise<any>;
    delete: (id: any) => Promise<any>;
}

interface UseEntityManagerOptions<T, CreateDto, UpdateDto> {
    service: EntityService<T, CreateDto, UpdateDto>;
    queryKey: QueryKey;
    translations?: {
        createSuccess?: string;
        createError?: string;
        updateSuccess?: string;
        updateError?: string;
        deleteSuccess?: string;
        deleteError?: string;
    };
}

export const useEntityManager = <T, CreateDto = Partial<T>, UpdateDto = Partial<T>>({
    service,
    queryKey,
    translations,
}: UseEntityManagerOptions<T, CreateDto, UpdateDto>) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const toast = useToast();

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingItem, setEditingItem] = useState<T | null>(null);
    const [itemToDelete, setItemToDelete] = useState<T | null>(null);

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

    const openCreate = () => {
        setEditingItem(null);
        setIsFormOpen(true);
    };

    const openEdit = (item: T) => {
        setEditingItem(item);
        setIsFormOpen(true);
    };

    const closeForm = () => {
        setIsFormOpen(false);
        setEditingItem(null);
    };

    const confirmDelete = (item: T) => {
        setItemToDelete(item);
    };

    const cancelDelete = () => {
        setItemToDelete(null);
    };

    const processCreate = (data: CreateDto) => {
        createMutation.mutate(data);
    };

    const processUpdate = (id: any, data: UpdateDto) => {
        updateMutation.mutate({ id, data });
    };

    const processDelete = (id: any) => {
        deleteMutation.mutate(id);
    };

    return {
        // State
        isFormOpen,
        editingItem,
        itemToDelete,

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
