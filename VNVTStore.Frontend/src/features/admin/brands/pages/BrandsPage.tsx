import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Tag } from 'lucide-react';
import { Modal, ConfirmDialog, Button, Badge } from '@/components/ui';
import { useEntityManager } from '@/hooks';
import { brandService, productService, type BrandDto, type CreateBrandRequest, type UpdateBrandRequest } from '@/services';
import { AdminPageHeader } from '@/components/admin';
import { StatsCards, type StatItem } from '@/components/admin/StatsCards';
import { useToast } from '@/store';
import { useAdminBrands, useBrandStats, ADMIN_BRAND_KEYS } from '../hooks/useAdminBrands';
import { BrandList } from '../components/BrandList';
import { BrandForm, type BrandFormData } from '../components/BrandForm';
import { PaginationDefaults } from '@/constants';
import { getImageUrl } from '@/utils/format';
import { useMutation, useQueryClient } from '@tanstack/react-query';

export const BrandsPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const queryClient = useQueryClient();
    
    // State
    const [page, setPage] = useState(PaginationDefaults.PAGE_INDEX);
    const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
    const [search, _setSearch] = useState('');
    const [viewingBrand, setViewingBrand] = useState<BrandDto | null>(null);
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [itemsToDelete, setItemsToDelete] = useState<BrandDto[] | null>(null);

    // Data Fetching
    const { data, isLoading, isFetching, refetch } = useAdminBrands({
        pageIndex: page,
        pageSize,
        search
    });

    const { data: _statsData, isLoading: _isStatsLoading } = useBrandStats();

    // Entity Manager
    const {
        isFormOpen,
        editingItem: editingBrand,
        itemToDelete: brandToDelete,
        isDeleting,
        openCreate,
        openEdit,
        closeForm,
        confirmDelete: baseConfirmDelete,
        cancelDelete,
        createMutation,
        updateMutation,
        deleteMutation
    } = useEntityManager<BrandDto, CreateBrandRequest, UpdateBrandRequest>({
        service: brandService,
        queryKey: ADMIN_BRAND_KEYS.all
    });

    const bulkDeleteMutation = useMutation({
        mutationFn: (codes: string[]) => brandService.deleteMultiple(codes),
        onSuccess: () => {
            setItemsToDelete(null);
            setSelectedIds(new Set());
            queryClient.invalidateQueries({ queryKey: ADMIN_BRAND_KEYS.all });
            toast.success(t('common.messages.deleteSuccess'));
        }
    });

    const confirmDelete = async (item: BrandDto) => {
        try {
            const result = await productService.search({ filters: [{ field: 'BrandCode', value: item.code }], pageSize: 1 });
            if (result.success && result.data && result.data.totalItems > 0) {
                toast.error(t('admin.brands.cannotDelete', { count: result.data.totalItems }));
                return;
            }
            baseConfirmDelete(item);
        } catch {
            baseConfirmDelete(item);
        }
    };

    const handleFormSubmit = async (formData: BrandFormData) => {
        if (editingBrand) {
            await updateMutation.mutateAsync({ id: editingBrand.code, data: formData as UpdateBrandRequest });
        } else {
            await createMutation.mutateAsync(formData as CreateBrandRequest);
        }
        closeForm();
        refetch();
    };

    const handleImport = async (file: File) => {
        try {
            await brandService.import(file);
            toast.success(t('common.messages.importSuccess'));
            refetch();
        } catch (error: unknown) {
            toast.error((error as Error).message);
        }
    };

    const stats: StatItem[] = [
        {
            label: t('common.fields.brand'),
            value: data?.totalItems || 0, 
            icon: <Tag size={24} />,
            color: 'blue',
            loading: isFetching
        }
    ];

    return (
        <div className="space-y-6">
            <AdminPageHeader
                title="admin.sidebar.brands"
                subtitle="admin.subtitles.brands"
            />

            <StatsCards stats={stats} />

            <BrandList
                brands={data?.items || []}
                isLoading={isLoading || isFetching}
                pagination={{
                    pageIndex: page,
                    pageSize,
                    totalItems: data?.totalItems || 0,
                    totalPages: data?.totalPages || 0
                }}
                onPageChange={setPage}
                onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
                onAdd={openCreate}
                onEdit={openEdit}
                onDelete={confirmDelete}
                onBulkDelete={setItemsToDelete}
                onView={setViewingBrand}
                onRefresh={refetch}
                onImport={handleImport}
                onExport={() => brandService.getAll(10000).then(res => res.data?.items || [])}
                selectedIds={selectedIds}
                onSelectionChange={setSelectedIds}
            />

            {isFormOpen && (
                <BrandForm
                    initialData={editingBrand ? {
                        name: editingBrand.name,
                        description: editingBrand.description || undefined,
                        logoUrl: editingBrand.logoUrl || undefined,
                        isActive: editingBrand.isActive
                    } : undefined}
                    onSubmit={handleFormSubmit}
                    onCancel={closeForm}
                    isLoading={createMutation.isPending || updateMutation.isPending}
                    modalOpen={isFormOpen}
                    modalTitle={editingBrand ? t('admin.actions.edit') : t('admin.actions.create')}
                    imageBaseUrl="https://example.com"
                />
            )}

            <ConfirmDialog
                isOpen={!!brandToDelete}
                onClose={cancelDelete}
                onConfirm={() => deleteMutation.mutate(brandToDelete!.code)}
                title={t('admin.actions.delete')}
                message={t('messages.confirmDelete', { name: brandToDelete?.name })}
                variant="danger"
                isLoading={isDeleting}
            />

            <ConfirmDialog
                isOpen={!!itemsToDelete}
                onClose={() => setItemsToDelete(null)}
                onConfirm={() => bulkDeleteMutation.mutate(itemsToDelete!.map(i => i.code))}
                title={t('admin.actions.delete')}
                message={t('messages.confirmDeleteCount', { count: itemsToDelete?.length || 0 })}
                variant="danger"
                isLoading={bulkDeleteMutation.isPending}
            />

            {viewingBrand && (
                <Modal
                    isOpen={!!viewingBrand}
                    onClose={() => setViewingBrand(null)}
                    title={t('admin.actions.viewDetails')}
                    size="md"
                >
                    <div className="space-y-6">
                        <div className="flex items-center gap-4 border-b border-gray-100 pb-4">
                            {viewingBrand.logoUrl ? (
                                <img src={getImageUrl(viewingBrand.logoUrl)} className="w-16 h-16 object-contain rounded-lg border border-gray-100" alt={viewingBrand.name} />
                            ) : (
                                <div className="w-16 h-16 bg-gray-50 rounded-lg flex items-center justify-center">
                                    <Tag className="text-gray-400" />
                                </div>
                            )}
                            <div>
                                <h2 className="text-xl font-bold">{viewingBrand.name}</h2>
                                <Badge color={viewingBrand.isActive !== false ? 'success' : 'secondary'} className="mt-1">
                                    {viewingBrand.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
                                </Badge>
                            </div>
                        </div>
                        <div>
                            <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.description')}</label>
                            <p className="text-sm mt-1">{viewingBrand.description || t('common.none')}</p>
                        </div>
                        <div className="flex justify-end pt-4">
                            <Button onClick={() => setViewingBrand(null)}>{t('common.close')}</Button>
                        </div>
                    </div>
                </Modal>
            )}
        </div>
    );
};

export default BrandsPage;
