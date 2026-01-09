import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Folder, Loader2, AlertCircle, Search } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog, Pagination } from '@/components/ui';
import { useToast } from '@/store';
import { useCategories } from '@/hooks';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '@/services/productService';

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

  // Pagination & Search
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const pageSize = 10;

  // Fetch categories
  const { data: categories = [], isLoading, isError, isFetching } = useCategories();

  // Filter by search
  const filteredCategories = (categories as Category[]).filter((c) =>
    c.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    c.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Paginate
  const totalItems = filteredCategories.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const paginatedCategories = filteredCategories.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null);

  // Form State
  const [formData, setFormData] = useState({
    name: '',
    description: '',
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: { name: string; description?: string }) =>
      categoryService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('messages.createSuccess'));
      setIsFormOpen(false);
      resetForm();
    },
    onError: (error: any) => {
      toast.error(error?.message || t('messages.createError'));
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: { code: string; data: { name: string; description?: string } }) =>
      categoryService.update(data.code, data.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('messages.updateSuccess'));
      setIsFormOpen(false);
      setEditingCategory(null);
      resetForm();
    },
    onError: (error: any) => {
      toast.error(error?.message || t('messages.updateError'));
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => categoryService.delete(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success(t('messages.deleteSuccess'));
      setCategoryToDelete(null);
    },
    onError: (error: any) => {
      toast.error(error?.message || t('messages.deleteError'));
    },
  });

  const resetForm = () => {
    setFormData({ name: '', description: '' });
  };

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
      createMutation.mutate({
        name: formData.name,
        description: formData.description,
      });
    }
  }, [formData, editingCategory, createMutation, updateMutation]);

  const handleDelete = () => {
    if (categoryToDelete) {
      deleteMutation.mutate(categoryToDelete.code);
    }
  };

  const showError = isError && !isLoading;
  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.categories')}</h1>
        <Button leftIcon={<Plus size={20} />} onClick={openCreateModal}>
          {t('messages.addCategory')}
        </Button>
      </div>

      {/* Search */}
      <div className="bg-primary rounded-xl p-4 border">
        <div className="max-w-md">
          <Input
            placeholder={t('messages.searchCategory')}
            leftIcon={<Search size={18} />}
            value={searchQuery}
            onChange={(e) => { setSearchQuery(e.target.value); setCurrentPage(1); }}
          />
        </div>
      </div>

      {/* Table */}
      <div className="bg-primary rounded-xl overflow-hidden shadow-sm border relative min-h-[400px]">
        {/* Loading overlay */}
        {(isLoading || isFetching) && (
          <div className="absolute inset-0 bg-primary/70 flex items-center justify-center z-10">
            <div className="flex items-center gap-3">
              <Loader2 className="w-6 h-6 animate-spin text-indigo-600" />
              <span className="text-secondary text-sm">{t('common.loading')}</span>
            </div>
          </div>
        )}

        {/* Error overlay */}
        {showError && (
          <div className="absolute inset-0 bg-primary flex items-center justify-center z-10">
            <div className="text-center">
              <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
              <h3 className="font-semibold mb-2">{t('messages.error')}</h3>
            </div>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full min-w-[600px]">
            <thead className="bg-secondary border-b">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold">{t('messages.name')}</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">{t('messages.description')}</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">{t('messages.status')}</th>
                <th className="px-4 py-3 text-right text-sm font-semibold">{t('messages.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {paginatedCategories.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-4 py-12 text-center text-secondary">
                    <Folder className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                    <p>{t('messages.noCategories')}</p>
                  </td>
                </tr>
              ) : (
                paginatedCategories.map((category) => (
                  <tr key={category.code} className="border-b last:border-0 hover:bg-secondary/50 transition-colors">
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                          <Folder className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                        </div>
                        <div>
                          <p className="font-medium">{category.name}</p>
                          <p className="text-xs text-tertiary">{category.code}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-4 text-secondary text-sm max-w-xs truncate">
                      {category.description || '-'}
                    </td>
                    <td className="px-4 py-4 text-center">
                      <Badge color={category.isActive !== false ? 'success' : 'secondary'} size="sm" variant="outline">
                        {category.isActive !== false ? 'Active' : 'Inactive'}
                      </Badge>
                    </td>
                    <td className="px-4 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => openEditModal(category)}
                          className="p-2 hover:bg-secondary rounded-lg transition-colors"
                          title={t('messages.edit')}
                        >
                          <Edit2 size={16} className="text-primary" />
                        </button>
                        <button
                          onClick={() => setCategoryToDelete(category)}
                          className="p-2 hover:bg-error/10 rounded-lg transition-colors"
                          title={t('messages.delete')}
                        >
                          <Trash2 size={16} className="text-error" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
          />
        )}
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => { setIsFormOpen(false); setEditingCategory(null); resetForm(); }}
        title={editingCategory ? t('messages.editCategory') : t('messages.addCategory')}
        size="md"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label={t('messages.categoryName')}
            placeholder={t('messages.enterCategoryName')}
            value={formData.name}
            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
            required
          />
          <Input
            label={t('messages.description')}
            placeholder={t('messages.enterDescription')}
            value={formData.description}
            onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => { setIsFormOpen(false); setEditingCategory(null); resetForm(); }}
            >
              {t('common.cancel')}
            </Button>
            <Button type="submit" isLoading={isSubmitting} disabled={!formData.name.trim()}>
              {editingCategory ? t('messages.update') : t('messages.create')}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!categoryToDelete}
        onClose={() => setCategoryToDelete(null)}
        onConfirm={handleDelete}
        title={t('messages.confirmDelete')}
        message={
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {t('messages.confirmDeleteCategory', { name: categoryToDelete?.name })}
          </p>
        }
        confirmText={t('messages.delete')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default CategoriesPage;
