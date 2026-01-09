import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Folder, RefreshCw } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog, Select } from '@/components/ui';
import { useToast } from '@/store';
import { useCategories } from '@/hooks';
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

export const CategoriesPage = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const toast = useToast();

  // Fetch categories
  const { data: categories = [], isLoading, isError, isFetching, error } = useCategories();

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null);

  // Form State
  const [formData, setFormData] = useState({ name: '', description: '', parentCode: '' });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: { name: string; description?: string }) => categoryService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('common.createSuccess'));
      setIsFormOpen(false);
      resetForm();
    },
    onError: (error: any) => toast.error(error?.message || t('common.createError')),
  });

  const updateMutation = useMutation({
    mutationFn: ({ code, data }: { code: string; data: { name: string; description?: string } }) =>
      categoryService.update(code, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('common.updateSuccess'));
      setIsFormOpen(false);
      setEditingCategory(null);
      resetForm();
    },
    onError: (error: any) => toast.error(error?.message || t('common.updateError')),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => categoryService.delete(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('common.deleteSuccess'));
      setCategoryToDelete(null);
    },
    onError: (error: any) => toast.error(error?.message || t('common.deleteError')),
  });

  const resetForm = () => setFormData({ name: '', description: '', parentCode: '' });

  const openCreateModal = () => {
    setEditingCategory(null);
    resetForm();
    setIsFormOpen(true);
  };

  const openEditModal = (category: Category) => {
    setEditingCategory(category);
    setFormData({
      name: category.name,
      description: category.description || '',
      parentCode: category.parentCode || '',
    });
    setIsFormOpen(true);
  };

  const handleSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim()) return;

    if (editingCategory) {
      updateMutation.mutate({
        code: editingCategory.code,
        data: { name: formData.name, description: formData.description },
      });
    } else {
      createMutation.mutate({ name: formData.name, description: formData.description });
    }
  }, [formData, editingCategory, createMutation, updateMutation]);

  const handleDelete = () => {
    if (categoryToDelete) deleteMutation.mutate(categoryToDelete.code);
  };

  const isSubmitting = createMutation.isPending || updateMutation.isPending;

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
        <Badge color={category.isActive ? 'success' : 'secondary'} size="sm">
          {category.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      sortable: true,
      className: 'text-center',
      headerClassName: 'text-center',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.categories')}</h1>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => queryClient.invalidateQueries({ queryKey: ['categories'] })}
            leftIcon={<RefreshCw size={16} className={isFetching ? 'animate-spin' : ''} />}
          >
            {t('common.refresh')}
          </Button>
          <Button leftIcon={<Plus size={20} />} onClick={openCreateModal}>
            {t('admin.actions.create')}
          </Button>
        </div>
      </div>

      {/* DataTable */}
      <DataTable
        columns={columns}
        data={categories as Category[]}
        keyField="code"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}
        onAdd={openCreateModal}
        onEdit={openEditModal}
        onDelete={(category) => setCategoryToDelete(category)}
        exportFilename="categories_export"
        searchOptions={[
          { label: t('admin.columns.name'), value: 'name' },
          { label: t('admin.columns.description'), value: 'description' },
        ]}
        emptyMessage={t('common.noResults')}
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => { setIsFormOpen(false); setEditingCategory(null); }}
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
              onClick={() => { setIsFormOpen(false); setEditingCategory(null); }}
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
        onClose={() => setCategoryToDelete(null)}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: 1 })}
        confirmText={t('common.delete')}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default CategoriesPage;
