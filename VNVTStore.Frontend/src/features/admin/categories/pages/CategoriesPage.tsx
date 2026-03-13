import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Folder, RefreshCw } from 'lucide-react';
import { Modal, ConfirmDialog, Button, Badge } from '@/components/ui';
import { useEntityManager } from '@/hooks';
import { categoryService, productService, type CategoryDto, type CreateCategoryRequest, type UpdateCategoryRequest } from '@/services';
import { AdminPageHeader } from '@/components/admin';
import { StatsCards, type StatItem } from '@/components/admin/StatsCards';
import { useToast } from '@/store';
import { useAdminCategories, useCategoryStats, ADMIN_CATEGORY_KEYS } from '../hooks/useAdminCategories';
import { CategoryList } from '../components/CategoryList';
import { CategoryForm, type CategoryFormData } from '../components/CategoryForm';
import { PaginationDefaults } from '@/constants';
import { getImageUrl } from '@/utils/format';


export const CategoriesPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    
    // State
    const [page, setPage] = useState(PaginationDefaults.PAGE_INDEX);
    const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
    const [search, _setSearch] = useState('');
    const [viewingCategory, setViewingCategory] = useState<CategoryDto | null>(null);
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

    const apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';

    // Data Fetching
    const { data, isLoading, isFetching, refetch } = useAdminCategories({
        pageIndex: page,
        pageSize,
        search
    });

    const { data: statsData, isLoading: isStatsLoading } = useCategoryStats();

    // Entity Manager
    const {
        isFormOpen,
        editingItem: editingCategory,
        itemToDelete: categoryToDelete,
        isDeleting,
        openCreate,
        openEdit,
        closeForm,
        confirmDelete: baseConfirmDelete,
        cancelDelete,
        createMutation,
        updateMutation,
        deleteMutation
    } = useEntityManager<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>({
        service: categoryService,
        queryKey: ADMIN_CATEGORY_KEYS.all
    });

    const confirmDelete = async (item: CategoryDto) => {
        try {
            const result = await productService.search({ filters: [{ field: 'CategoryCode', value: item.code }], pageSize: 1 });
            if (result.success && result.data && result.data.totalItems > 0) {
                toast.error(t('admin.categories.cannotDelete', { count: result.data.totalItems }));
                return;
            }
            baseConfirmDelete(item);
        } catch {
            baseConfirmDelete(item);
        }
    };

    const handleFormSubmit = async (formData: CategoryFormData) => {
        const payload = {
            name: formData.name,
            description: formData.description,
            parentCode: formData.parentCode || null,
            imageURL: formData.imageURL,
            isActive: formData.isActive
        };

        if (editingCategory) {
            await updateMutation.mutateAsync({ id: editingCategory.code, data: payload as UpdateCategoryRequest });
        } else {
            await createMutation.mutateAsync(payload as CreateCategoryRequest);
        }
        closeForm();
        refetch();
    };

    const handleImport = async (file: File) => {
        try {
            await categoryService.import(file);
            toast.success(t('common.messages.importSuccess'));
            refetch();
        } catch (error: unknown) {
            toast.error((error as Error).message);
        }
    };

    const stats: StatItem[] = [
        {
            label: t('admin.stats.totalCategories'),
            value: statsData?.total || 0,
            icon: <Folder size={24} />,
            color: 'indigo',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.mainCategories'),
            value: statsData?.main || 0,
            icon: <Folder size={24} />, 
            color: 'emerald',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.active'),
            value: statsData?.active || 0, 
            icon: <RefreshCw size={24} />,
            color: 'amber',
            loading: isStatsLoading
        }
    ];

    return (
        <div className="space-y-6">
            <AdminPageHeader
                title="admin.sidebar.categories"
                subtitle="admin.subtitles.categories"
            />

            <StatsCards stats={stats} />

            <CategoryList
                categories={data?.items || []}
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
                onView={setViewingCategory}
                onRefresh={refetch}
                onImport={handleImport}
                onExport={() => categoryService.exportData()}
                selectedIds={selectedIds}
                onSelectionChange={setSelectedIds}
            />

            {isFormOpen && (
                <CategoryForm
                    initialData={editingCategory ? {
                        name: editingCategory.name,
                        description: editingCategory.description || undefined,
                        parentCode: editingCategory.parentCode || undefined,
                        imageURL: editingCategory.imageURL || undefined,
                        isActive: editingCategory.isActive
                    } : undefined}
                    onSubmit={handleFormSubmit}
                    onCancel={closeForm}
                    isLoading={createMutation.isPending || updateMutation.isPending}
                    modalOpen={isFormOpen}
                    modalTitle={editingCategory ? t('admin.actions.edit') : t('admin.actions.create')}
                    excludeCode={editingCategory?.code}
                    imageBaseUrl={apiBaseUrl.replace(/\/api\/v1\/?$/, '')}
                />
            )}

            <ConfirmDialog
                isOpen={!!categoryToDelete}
                onClose={cancelDelete}
                onConfirm={() => deleteMutation.mutate(categoryToDelete!.code)}
                title={t('admin.actions.delete')}
                message={t('messages.confirmDelete', { name: categoryToDelete?.name })}
                variant="danger"
                isLoading={isDeleting}
            />

            {viewingCategory && (
                <Modal
                    isOpen={!!viewingCategory}
                    onClose={() => setViewingCategory(null)}
                    title={t('admin.actions.viewDetails')}
                    size="md"
                >
                    <div className="space-y-4">
                        {viewingCategory.imageURL && (
                            <div className="relative w-full h-48 rounded-lg overflow-hidden border border-gray-100 dark:border-gray-700">
                                <img 
                                    src={getImageUrl(viewingCategory.imageURL)} 
                                    alt={viewingCategory.name} 
                                    className="w-full h-full object-cover"
                                />
                            </div>
                        )}
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.code')}</label>
                                <p className="font-medium">{viewingCategory.code}</p>
                            </div>
                            <div>
                                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.name')}</label>
                                <p className="font-medium">{viewingCategory.name}</p>
                            </div>
                            <div className="col-span-2">
                                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.description')}</label>
                                <p className="text-gray-600 dark:text-gray-300">{viewingCategory.description || t('common.none')}</p>
                            </div>
                            <div>
                                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.status')}</label>
                                <Badge color={viewingCategory.isActive !== false ? 'success' : 'secondary'} className="block mt-1 w-fit">
                                    {viewingCategory.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
                                </Badge>
                            </div>
                        </div>
                        <div className="flex justify-end pt-4">
                            <Button onClick={() => setViewingCategory(null)}>{t('common.close')}</Button>
                        </div>
                    </div>
                </Modal>
            )}
        </div>
    );
};

export default CategoriesPage;
