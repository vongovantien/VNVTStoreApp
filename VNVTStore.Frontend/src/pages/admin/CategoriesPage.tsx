
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Folder, RefreshCw } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { useCategories, useCategoriesList, useEntityManager } from '@/hooks';
import { categoryService, type CategoryDto, type CreateCategoryRequest, type UpdateCategoryRequest } from '@/services';
import { DataTable, type DataTableColumn } from '@/components/common';
import { AdminPageHeader } from '@/components/admin';
import { CategoryForm, type CategoryFormData } from './forms';
import { PaginationDefaults, API_ENDPOINTS } from '@/constants';
import { CATEGORY_LIST_FIELDS } from '@/constants/fieldConstants';
import { getImageUrl } from '@/utils/format';

export default function CategoriesPage() {
  const { t } = useTranslation();
  
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
    fields: CATEGORY_LIST_FIELDS,  // Selective columns for list view
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
    confirmDelete,
    cancelDelete,
    createMutation,
    updateMutation,
    deleteMutation
  } = useEntityManager<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>({
    service: categoryService,
    queryKey: ['categories'] // Update query key to include pagination
  });

  const { mutate: createCategory, isPending: isCreating } = createMutation;
  const { mutate: updateCategory, isPending: isUpdating } = updateMutation;
  const { mutate: deleteCategory } = deleteMutation;

  // View State
  const [viewingCategory, setViewingCategory] = useState<CategoryDto | null>(null);

  // Selection State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

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
          description: formData.description,
          parentCode: formData.parentCode || null,
          imageUrl: formData.imageUrl,
          isActive: formData.isActive
        }
      });
    } else {
      await createCategory({
        name: formData.name,
        description: formData.description,
        parentCode: formData.parentCode || null,
        imageUrl: formData.imageUrl,
        isActive: formData.isActive
      });
    }
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
      id: 'imageUrl',
      header: t('common.fields.image'),
      width: '80px',
      className: 'text-center',
      accessor: (category) => {

        return (
        <div className="flex flex-col items-center">
            <div className="w-10 h-10 rounded-lg overflow-hidden bg-gray-100 dark:bg-slate-700 border border-gray-200 dark:border-slate-600">
               {category.imageUrl ? (
                 <img 
                   src={getImageUrl(category.imageUrl)} 
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
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (category) => (
        <Badge color={category.isActive !== false ? 'success' : 'secondary'}>
          {category.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
        </Badge>
      )
    },
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
          imageUrl: editingCategory.imageUrl || undefined,
          isActive: editingCategory.isActive
      };
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.categories"
        subtitle="admin.subtitles.categories"
      />

      <DataTable
        data={categories}
        columns={columns}
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
        onAdvancedSearch={(filters) => {
             // Basic search simulation
             refetch(); // In real app, pass filters to hook
        }}
        keyField="code"
        enableSelection
        onAdd={() => openCreate()}
        onRefresh={() => refetch()}
        
        // Selection & Actions
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onEdit={(item) => openEdit(item)}
        onDelete={(item) => confirmDelete(item)}
        onBulkDelete={() => { /* categories doesn't seem to support bulk delete in hook yet? */ }}
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
            initialData={prepareInitialData()}
            onSubmit={handleFormSubmit}
            onCancel={closeForm}
            isLoading={isSubmitting}
            modalOpen={isFormOpen}
            modalTitle={editingCategory ? t('admin.actions.edit') : t('admin.actions.create')}
            excludeCode={editingCategory?.code}
            imageBaseUrl={apiBaseUrl.replace(/\/api\/v1\/?$/, '')}
        />
       )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!categoryToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete', { name: categoryToDelete?.name })}
        confirmText={t('admin.actions.delete')}
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
            {viewingCategory.imageUrl && (
              <div className="relative w-full h-48 rounded-lg overflow-hidden border border-gray-100 dark:border-gray-700">
                <img 
                  src={getImageUrl(viewingCategory.imageUrl)} 
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
                          {viewingCategory.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
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
    </div>
  );
}
