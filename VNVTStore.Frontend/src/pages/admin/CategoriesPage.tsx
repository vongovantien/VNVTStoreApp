import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Folder, RefreshCw, Edit, Trash2, Eye } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog, Select } from '@/components/ui';
import { useToast } from '@/store';
import { useCategories, useCategoriesList, useEntityManager } from '@/hooks';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '@/services/productService';
import { DataTable, type DataTableColumn } from '@/components/common';

interface Category {
  code: string;
  name: string;
  description?: string;
  parentCode?: string;
  isActive?: boolean;
  productCount?: number;
}

import { PageSize, PaginationDefaults } from '@/constants';

export const CategoriesPage = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState<number>(PageSize.DEFAULT);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Fetch categories
  const {
    data: categoriesData,
    isLoading,
    isError,
    isFetching,
    error,
    refetch
  } = useCategoriesList({
    pageIndex: currentPage,
    pageSize,
    search: advancedFilters.name,
    parentCode: advancedFilters.parentCode,
    isActive: advancedFilters.isActive
  });

  const categories = categoriesData?.categories || [];
  const totalPages = categoriesData?.totalPages || 1;
  const totalItems = categoriesData?.totalItems || 0;

  // Helper for dropdown (get all categories) - simpler version or reuse search with large page size
  const { data: allCategories = [] } = useCategories();

  const {
    // State
    isFormOpen,
    editingItem: editingCategory,
    itemToDelete: categoryToDelete,
    isLoading: isSubmitting,
    isDeleting,

    // Actions
    openCreate,
    openEdit,
    closeForm,
    confirmDelete,
    cancelDelete,

    // Operations
    create: createCategory,
    update: updateCategory,
    delete: deleteCategory,
    deleteMutation
  } = useEntityManager<Category, { name: string; description?: string }, { name: string; description?: string }>({
    service: categoryService,
    queryKey: ['categories'],
  });

  // Form State
  const [formData, setFormData] = useState({ name: '', description: '', parentCode: '' });
  const [viewingCategory, setViewingCategory] = useState<Category | null>(null);

  const resetForm = () => setFormData({ name: '', description: '', parentCode: '' });

  const handleOpenCreate = () => {
    openCreate();
    resetForm();
  };

  const handleOpenView = (category: Category) => {
    setViewingCategory(category);
  };

  const handleOpenEdit = (category: Category) => {
    openEdit(category);
    setFormData({
      name: category.name,
      description: category.description || '',
      parentCode: category.parentCode || '',
    });
  };

  const handleSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim()) return;

    if (editingCategory) {
      updateCategory(editingCategory.code, {
        name: formData.name,
        description: formData.description
      });
    } else {
      createCategory({
        name: formData.name,
        description: formData.description
      });
    }
  }, [formData, editingCategory, createCategory, updateCategory]);

  const handleDelete = () => {
    if (categoryToDelete) deleteCategory(categoryToDelete.code);
  };

  const handleReset = useCallback(() => {
    setAdvancedFilters({});
    setCurrentPage(1);
    refetch();
  }, [refetch]);

  // Column definitions
  const columns: DataTableColumn<Category>[] = [
    {
      id: 'name',
      header: t('admin.columns.name'),
      accessor: (category) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-lg bg-secondary flex items-center justify-center">
            <Folder size={20} className="text-primary" />
          </div>
          <div>
            <p className="font-medium">{category.name}</p>
            {category.description && (
              <p className="text-xs text-tertiary truncate max-w-[200px]">{category.description}</p>
            )}
          </div>
        </div>
      ),
      sortable: true,
    },
    {
      id: 'parentCategory',
      header: t('admin.columns.parentCategory'),
      accessor: (category) => {
        const parent = (allCategories as Category[]).find(c => c.code === category.parentCode);
        return parent ? (
          <Badge variant="soft">
            {parent.name}
          </Badge>
        ) : (
          <span className="text-tertiary italic text-sm">-</span>
        );
      },
      sortable: true,
    },
    {
      id: 'productCount',
      header: t('admin.columns.productCount'),
      accessor: (category) => (
        <span className="font-medium">{category.productCount || 0}</span>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
    },
    {
      id: 'isActive',
      header: t('admin.columns.status'),
      accessor: (category) => (
        <Badge color={category.isActive ? 'success' : 'error'} size="sm">
          {category.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      sortable: true,
      className: 'text-center',
      headerClassName: 'text-center',
    },
    {
      id: 'actions',
      header: t('admin.columns.action'),
      accessor: (category) => (
        <div className="flex items-center justify-center gap-2">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            title={t('admin.actions.view')}
            onClick={() => handleOpenView(category)}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            title={t('admin.actions.edit')}
            onClick={() => handleOpenEdit(category)}
          >
            <Edit size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            title={t('admin.actions.delete')}
            onClick={() => confirmDelete(category)}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.categories')}</h1>

      <DataTable
        columns={columns}
        data={categories as Category[]}
        keyField="code"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}
        onAdd={handleOpenCreate}
        onEdit={handleOpenEdit}
        onView={handleOpenView}
        onDelete={confirmDelete}
        onReset={handleReset}
        exportFilename="categories_export"

        // Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setCurrentPage(1);
        }}

        // Filters
        onAdvancedSearch={(filters) => {
          setAdvancedFilters(filters);
          setCurrentPage(1);
        }}
        searchOptions={[
          { label: t('admin.columns.name'), value: 'name' },
          { label: t('admin.columns.description'), value: 'description' },
          { label: t('admin.columns.parentCategory'), value: 'parentCode' },
        ]}
        advancedFilterDefs={[
          {
            id: 'name',
            label: t('admin.columns.name'),
            type: 'text',
            placeholder: 'Tên danh mục...'
          },
          {
            id: 'description',
            label: t('admin.columns.description'),
            type: 'text',
          },
          {
            id: 'parentCode',
            label: t('admin.columns.parentCategory'),
            type: 'select',
            options: (categories as Category[]).map(c => ({ label: c.name, value: c.code }))
          },
          {
            id: 'isActive',
            label: t('admin.columns.status'),
            type: 'select',
            options: [
              { value: 'true', label: t('common.status.active') },
              { value: 'false', label: t('common.status.inactive') }
            ]
          }
        ]}
        enableColumnVisibility={true}
        emptyMessage={t('common.noResults')}
      />

      {/* View Modal */}
      <Modal
        isOpen={!!viewingCategory}
        onClose={() => setViewingCategory(null)}
        title={t('admin.actions.view') + ' ' + t('admin.categories')}
      >
        {viewingCategory && (
          <div className="space-y-6">
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-lg bg-emerald-100 flex items-center justify-center text-emerald-600">
                <Folder size={32} />
              </div>
              <div>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{viewingCategory.name}</h2>
                <Badge color={viewingCategory.isActive ? 'success' : 'error'} size="sm" className="mt-1">
                  {viewingCategory.isActive ? t('common.status.active') : t('common.status.inactive')}
                </Badge>
              </div>
            </div>

            <div className="space-y-4">
              <div>
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2 mb-2">{t('admin.columns.description')}</h3>
                <p className="text-slate-600 dark:text-slate-300 text-sm">{viewingCategory.description || t('common.noDescription')}</p>
              </div>

              <div>
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2 mb-2">{t('admin.columns.parentCategory')}</h3>
                <p className="text-slate-600 dark:text-slate-300 text-sm">
                  {(allCategories as Category[]).find(c => c.code === viewingCategory.parentCode)?.name || '-'}
                </p>
              </div>
            </div>

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingCategory(null)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingCategory ? t('admin.actions.edit') : t('admin.actions.create')}
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label={t('admin.columns.name') + ' *'}
            placeholder={t('admin.columns.name')}
            value={formData.name}
            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
            required
          />
          <Input
            label={t('admin.columns.description')}
            placeholder={t('admin.columns.description')}
            value={formData.description}
            onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
          />
          <Select
            label={t('admin.columns.parentCategory')}
            options={[
              { value: '', label: t('common.all') },
              ...(categories as Category[])
                .filter((c) => c.code !== editingCategory?.code)
                .map((c) => ({ value: c.code, label: c.name }))
            ]}
            value={formData.parentCode}
            onChange={(e) => setFormData(prev => ({ ...prev, parentCode: e.target.value }))}
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={closeForm}
            >
              {t('common.cancel')}
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              {editingCategory ? t('common.update') : t('common.create')}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!categoryToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: 1 })}
        confirmText={t('common.delete')}
        isLoading={isDeleting}
      />
    </div>
  );
};

export default CategoriesPage;
