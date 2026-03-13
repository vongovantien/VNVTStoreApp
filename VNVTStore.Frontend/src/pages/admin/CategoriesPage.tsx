
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Folder, RefreshCw } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { useCategoriesList, useEntityManager } from '@/hooks';
import { categoryService, productService, type CategoryDto, type CreateCategoryRequest, type UpdateCategoryRequest } from '@/services';
import { DataTable, type DataTableColumn, CommonColumns } from '@/components/common';
import { AdminPageHeader } from '@/components/admin';
import { CategoryForm, type CategoryFormData } from './forms';
import { PaginationDefaults } from '@/constants';
import { CATEGORY_LIST_FIELDS } from '@/constants/fieldConstants';
import { getImageUrl } from '@/utils/format';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useToast } from '@/store';

export default function CategoriesPage() {
  const { t } = useTranslation();
  const toast = useToast();
  
  // Pagination State
  const [pagination, setPagination] = useState({
    pageIndex: PaginationDefaults.PAGE_INDEX,
    pageSize: PaginationDefaults.PAGE_SIZE
  });

  // Get API Base URL for image previews
  const apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';

  // Data Fetching with Pagination
  const { 
    data, 
    isLoading, 
    refetch 
  } = useCategoriesList({
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    fields: CATEGORY_LIST_FIELDS, 
  });

  const categories = data?.categories || [];
  const totalItems = data?.totalItems || 0;
  const totalPages = data?.totalPages || 0;

  // Entity Manager (for CRUD state)
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
    queryKey: ['categories'] 
  });

  const confirmDelete = async (item: CategoryDto) => {
    try {
        const result = await productService.search({ filters: [{ field: 'CategoryCode', value: item.code }], pageSize: 1 });
        if (result.success && result.data && result.data.totalItems > 0) {
             toast.error(t('admin.categories.cannotDelete', { 
                 count: result.data.totalItems, 
                 defaultValue: `Cannot delete: Category contains ${result.data.totalItems} products.` 
             }));
             return;
        }
        baseConfirmDelete(item);
    } catch (error) {
        console.error("Check dependency failed", error);
        baseConfirmDelete(item);
    }
  };

  const { mutate: createCategory, isPending: isCreating } = createMutation;
  const { mutate: updateCategory, isPending: isUpdating } = updateMutation;
  const { mutate: deleteCategory } = deleteMutation;

  // View State
  const [viewingCategory, setViewingCategory] = useState<CategoryDto | null>(null);

  // Selection State
  // Selection State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [itemsToDelete, setItemsToDelete] = useState<CategoryDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => categoryService.deleteMultiple(codes),
    onSuccess: () => {
        toast.success(t('common.deleteSuccess'));
        setItemsToDelete(null);
        setSelectedIds(new Set());
        refetch();
    },
    onError: (err: Error) => {
        toast.error(err.message || t('common.deleteError'));
    }
  });

  const handleBulkDelete = (items: CategoryDto[]) => {
      setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
        bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

  const handlePageChange = (page: number) => {
    setPagination(prev => ({ ...prev, pageIndex: page }));
  };

  const handlePageSizeChange = (size: number) => {
    setPagination(prev => ({ ...prev, pageSize: size, pageIndex: 1 }));
  };

  const handleFormSubmit = async (formData: CategoryFormData) => {
    if (editingCategory) {
      await updateCategory({
        id: editingCategory.code,
        data: {
          name: formData.name,
          description: formData.description || '',
          parentCode: formData.parentCode || null,
          imageURL: formData.imageURL || '',
          isActive: formData.isActive
        }
      });
    } else {
      await createCategory({
        name: formData.name,
        description: formData.description || '',
        parentCode: formData.parentCode || null,
        imageURL: formData.imageURL || '',
        isActive: formData.isActive
      });
    }
  };


  const importMutation = useMutation({
    mutationFn: (file: File) => categoryService.import(file),
    onSuccess: () => {
      toast.success(t('common.messages.importSuccess') || 'Import successful');
      refetch();
    },
    onError: (error: Error) => {
      toast.error(error.message || t('common.messages.importError') || 'Import failed');
    },
  });

  const handleImport = async (file: File) => {
    await importMutation.mutateAsync(file);
  };

  const handleDelete = () => {
    if (categoryToDelete) {
      deleteCategory(categoryToDelete.code, {
        onSuccess: () => refetch()
      });
    }
  };

  const isSubmitting = isCreating || isUpdating;

  // Column definitions for DataTable
  const columns: DataTableColumn<CategoryDto>[] = [
    {
      id: 'imageURL',
      header: t('common.fields.image'),
      width: '120px',
      className: 'text-center',
      accessor: (category) => {

        return (
        <div className="flex flex-col items-center">
            <div className="w-10 h-10 rounded-lg overflow-hidden bg-gray-100 dark:bg-slate-700 border border-gray-200 dark:border-slate-600">
               {category.imageURL ? (
                 <img 
                   src={getImageUrl(category.imageURL)} 
                   alt={category.name} 
                   className="w-full h-full object-cover"
                   onError={(e) => console.error("Img Error:", e.currentTarget.src)}
                 />
               ) : (
                 <div className="w-full h-full flex items-center justify-center text-gray-400">
                   <Folder size={18} />
                 </div>
               )}
            </div>

        </div>
      );
    }
  },
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: 'name',
    },
    { 
      id: 'description',
      header: t('common.fields.description'),
      accessor: 'description',
      className: 'hidden md:table-cell text-gray-500' 
    },
    {
      id: 'parentCode',
      header: t('common.fields.parentCategory'),
      accessor: (category) => {
        if (!category.parentCode) return <span className="text-gray-400 italic">{t('common.none')}</span>;
        return (
          <Badge variant="outline" className="gap-1">
            <Folder size={10} className="mr-1" />
            {category.parentCode} 
          </Badge>
        );
      }
    },
    CommonColumns.createStatusColumn(t),
    {
      id: 'actions',
      header: t('common.fields.action'),
      width: '100px',
      className: 'w-[100px]',
      accessor: (category) => (
        <TableActions
          onView={() => setViewingCategory(category)}
          onEdit={() => openEdit(category)}
          onDelete={() => confirmDelete(category)}
        />
      )
    }
  ];

  const prepareInitialData = (): Partial<CategoryFormData> | undefined => {
      if (!editingCategory) return undefined;
      return {
          name: editingCategory.name,
          description: editingCategory.description || undefined,
          parentCode: editingCategory.parentCode || undefined,
          imageURL: editingCategory.imageURL || undefined,
          isActive: editingCategory.isActive ?? true
      };
  };

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['category-stats'],
      queryFn: () => categoryService.getStats(),
      staleTime: 60000,
  });

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

      <DataTable
        data={categories}
        columns={columns}
        keyField="code"
        enableSelection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onBulkDelete={handleBulkDelete}
        isLoading={isLoading}
        searchPlaceholder={t('common.placeholders.search')}
        advancedFilterDefs={[
          {
            id: 'name',
            label: t('common.fields.name'),
            type: 'text',
            placeholder: t('common.placeholders.search')
          },
          {
            id: 'status',
            label: t('common.fields.status'),
            type: 'select',
            options: [
              { value: 'active', label: t('admin.status.active') },
              { value: 'inactive', label: t('admin.status.inactive') }
            ]
          }
        ]}
        onAdvancedSearch={() => {
             // Basic search simulation
             refetch(); // In real app, pass filters to hook
        }}
        onImport={handleImport}
        importTemplateUrl="/categories/template"
        importTitle={t('common.importData')}
        onExportAllData={() => categoryService.exportData()}
        onAdd={() => openCreate()}
        onRefresh={() => refetch()}
        onView={(item) => setViewingCategory(item)}

        // Server-Side Pagination
        currentPage={pagination.pageIndex}
        totalItems={totalItems}
        totalPages={totalPages}
        pageSize={pagination.pageSize}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

       {/* Form Modal */}
       {isFormOpen && (
        <CategoryForm
            initialData={prepareInitialData() || {}}
            onSubmit={handleFormSubmit}
            onCancel={closeForm}
            isLoading={isSubmitting}
            modalOpen={isFormOpen}
            modalTitle={editingCategory ? t('admin.actions.edit') : t('admin.actions.create')}
            excludeCode={editingCategory?.code || ''}
            imageBaseUrl={apiBaseUrl.replace(/\/api\/v1\/?$/, '')}
        />
       )}
       
      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!categoryToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('common.actions.delete')}
        message={t('messages.confirmDelete', { name: categoryToDelete?.name })}
        confirmText={t('common.actions.delete')}
        cancelText={t('common.cancel')}
        variant="danger"
        isLoading={isDeleting}
      />

      {/* View Details Modal */}
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
              
              {viewingCategory.parentCode && (
                <div className="col-span-2">
                   <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.parentCategory')}</label>
                   <div className="mt-1">
                     <Badge variant="outline">
                       <Folder size={12} className="mr-1" />
                       {categories.find(c => c.code === viewingCategory.parentCode)?.name || viewingCategory.parentCode}
                     </Badge>
                   </div>
                </div>
              )}
              
              <div className="col-span-2">
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.description')}</label>
                <p className="text-gray-600 dark:text-gray-300">
                  {viewingCategory.description || t('common.none')}
                </p>
              </div>

               <div className="col-span-2">
                  <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.status')}</label>
                  <div className="mt-1">
                      <Badge color={viewingCategory.isActive !== false ? 'success' : 'secondary'}>
                          {viewingCategory.isActive !== false ? t('common.status.active') : t('common.status.inactive')}
                      </Badge>
                  </div>
              </div>
            </div>
            
            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingCategory(null)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        </Modal>
      )}
      <ConfirmDialog
        isOpen={!!itemsToDelete}
        onClose={() => setItemsToDelete(null)}
        onConfirm={confirmBulkDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDeleteCount', { count: itemsToDelete?.length || 0 })}
        confirmText={t('admin.actions.delete')}
        cancelText={t('common.cancel')}
        variant="danger"
        isLoading={bulkDeleteMutation.isPending}
      />
    </div>
  );
}
