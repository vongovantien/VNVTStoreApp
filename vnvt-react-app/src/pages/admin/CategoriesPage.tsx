import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Folder, Loader2, AlertCircle } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog } from '@/components/ui';
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

  // Fetch categories
  const { data: categories = [], isLoading, isError, isFetching } = useCategories();

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
      categoryService.createCategory(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success('Tạo danh mục thành công!');
      setIsFormOpen(false);
      resetForm();
    },
    onError: () => {
      toast.error('Không thể tạo danh mục');
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: { code: string; data: { name: string; description?: string } }) =>
      categoryService.updateCategory(data.code, data.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success('Cập nhật danh mục thành công!');
      setIsFormOpen(false);
      setEditingCategory(null);
      resetForm();
    },
    onError: () => {
      toast.error('Không thể cập nhật danh mục');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => categoryService.deleteCategory(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      toast.success('Xóa danh mục thành công!');
      setCategoryToDelete(null);
    },
    onError: () => {
      toast.error('Không thể xóa danh mục');
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
          Thêm danh mục
        </Button>
      </div>

      {/* Categories Grid */}
      <div className="bg-primary rounded-xl shadow-sm border relative min-h-[300px]">
        {/* Loading overlay */}
        {(isLoading || isFetching) && (
          <div className="absolute inset-0 bg-primary/70 flex items-center justify-center z-10">
            <div className="flex items-center gap-3">
              <Loader2 className="w-6 h-6 animate-spin text-indigo-600" />
              <span className="text-secondary text-sm">Đang tải...</span>
            </div>
          </div>
        )}

        {/* Error overlay */}
        {showError && (
          <div className="absolute inset-0 bg-primary flex items-center justify-center z-10">
            <div className="text-center">
              <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
              <h3 className="font-semibold mb-2">Có lỗi xảy ra</h3>
            </div>
          </div>
        )}

        <div className="p-6">
          {categories.length === 0 && !isLoading ? (
            <div className="text-center py-12">
              <Folder className="w-16 h-16 mx-auto mb-4 text-gray-300" />
              <p className="text-secondary">Chưa có danh mục nào</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {(categories as Category[]).map((category) => (
                <div
                  key={category.code}
                  className="p-4 bg-secondary rounded-lg border hover:border-indigo-500 transition-colors group"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="w-10 h-10 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                      <Folder className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                    </div>
                    <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button
                        onClick={() => openEditModal(category)}
                        className="p-1.5 hover:bg-tertiary rounded transition-colors"
                        title="Sửa"
                      >
                        <Edit2 size={14} />
                      </button>
                      <button
                        onClick={() => setCategoryToDelete(category)}
                        className="p-1.5 hover:bg-error/10 text-error rounded transition-colors"
                        title="Xóa"
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </div>
                  <h3 className="font-medium mb-1">{category.name}</h3>
                  {category.description && (
                    <p className="text-sm text-tertiary line-clamp-2 mb-2">{category.description}</p>
                  )}
                  <Badge color={category.isActive !== false ? 'success' : 'gray'} size="sm" variant="outline">
                    {category.isActive !== false ? 'Active' : 'Inactive'}
                  </Badge>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => { setIsFormOpen(false); setEditingCategory(null); resetForm(); }}
        title={editingCategory ? 'Sửa danh mục' : 'Thêm danh mục mới'}
        size="md"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Tên danh mục"
            placeholder="Nhập tên danh mục"
            value={formData.name}
            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
            required
          />
          <Input
            label="Mô tả"
            placeholder="Nhập mô tả (tùy chọn)"
            value={formData.description}
            onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => { setIsFormOpen(false); setEditingCategory(null); resetForm(); }}
            >
              Hủy
            </Button>
            <Button type="submit" isLoading={isSubmitting} disabled={!formData.name.trim()}>
              {editingCategory ? 'Cập nhật' : 'Tạo mới'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!categoryToDelete}
        onClose={() => setCategoryToDelete(null)}
        onConfirm={handleDelete}
        title="Xác nhận xóa"
        message={
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Bạn có chắc chắn muốn xóa danh mục <strong className="text-gray-900 dark:text-white">"{categoryToDelete?.name}"</strong>?
          </p>
        }
        confirmText="Xóa"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default CategoriesPage;
