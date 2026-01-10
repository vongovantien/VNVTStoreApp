import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Eye, Star } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { ProductForm, ProductFormData } from './forms/ProductForm';
import {
  useProducts,
  useCreateProduct,
  useUpdateProduct,
  useDeleteProduct,
} from '@/hooks';
import { useToast } from '@/store';
import type { Product } from '@/types';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';

// Types for sorting
type SortField = 'name' | 'price' | 'stock' | 'createdAt';
type SortDirection = 'asc' | 'desc';

export const ProductsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});
  const pageSize: number = 10;

  // Sorting
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>('desc');

  // Fetch API
  const {
    data: productsData,
    isLoading,
    isFetching,
    isError,
    error,
  } = useProducts({
    pageIndex: currentPage,
    pageSize,
    search: searchQuery || undefined,
    sortField,
    sortDir,
    ...advancedFilters
  });

  const products: Product[] = productsData?.products || [];
  const totalPages: number = productsData?.totalPages || 1;
  const totalItems: number = productsData?.totalItems || 0;

  // Mutations
  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();
  const deleteMutation = useDeleteProduct();

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [productToDelete, setProductToDelete] = useState<Product | null>(null);

  // Column Definitions
  const columns: DataTableColumn<Product>[] = [
    {
      id: 'name',
      header: t('admin.columns.name'),
      accessor: (product) => (
        <div className="flex items-center gap-3">
          <img
            src={product.images?.[0] || 'https://placehold.co/100?text=No+Image'}
            alt={product.name}
            className="w-12 h-12 rounded-lg object-cover border bg-white"
          />
          <div>
            <p className="font-medium text-sm text-slate-700 dark:text-slate-200">{product.name}</p>
            <p className="text-xs text-slate-500">{product.brand}</p>
          </div>
        </div>
      ),
      sortable: true
    },
    {
      id: 'category',
      header: t('admin.columns.category'),
      accessor: 'category',
    },
    {
      id: 'price',
      header: t('admin.columns.price'),
      accessor: (product) => (
        product.price > 0 ? (
          <span className="font-semibold text-rose-600">{formatCurrency(product.price)}</span>
        ) : (
          <Badge color="primary" size="sm">Liên hệ</Badge>
        )
      ),
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'stock',
      header: t('admin.columns.stock'),
      accessor: (product) => (
        <span className={product.stock > 10 ? 'text-emerald-600 font-medium' : product.stock > 0 ? 'text-amber-600 font-medium' : 'text-rose-600 font-medium'}>
          {product.stock}
        </span>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    },
    {
      id: 'rating',
      header: t('admin.columns.rating'),
      accessor: (product) => (
        <div className="flex items-center justify-center gap-1 text-sm">
          <Star size={16} className="text-amber-400 fill-amber-400" />
          <span>{product.rating} ({product.reviewCount})</span>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('admin.columns.status'),
      accessor: (product) => (
        <Badge
          color={product.isActive !== false ? 'success' : 'secondary'}
          size="sm"
          variant="outline"
        >
          {product.isActive !== false ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'actions',
      header: t('admin.columns.action'),
      accessor: (product) => (
        <div className="flex items-center justify-center gap-2">
          <button className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500" title="Xem">
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            title="Sửa"
            onClick={() => {
              setEditingProduct(product);
              setIsFormOpen(true);
            }}
          >
            <Edit2 size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            title="Xóa"
            onClick={() => setProductToDelete(product)}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    }
  ];

  // Handlers
  const handleSort = (field: string, dir: 'asc' | 'desc') => {
    setSortField(field as SortField);
    setSortDir(dir);
    setCurrentPage(1);
  };

  const handleAdvancedSearch = (filters: Record<string, string>) => {
    setAdvancedFilters(filters);
    setCurrentPage(1);
    // Extract text search if present
    if (filters.search) {
      setSearchQuery(filters.search);
    }
  };

  const handleCreate = async (data: ProductFormData) => {
    try {
      const result = await createMutation.mutateAsync({
        name: data.name,
        description: data.description,
        price: data.price,
        categoryId: data.categoryId,
        stockQuantity: data.stock,
        color: data.color,
        power: data.power,
        voltage: data.voltage,
        material: data.material,
        size: data.size,
      });
      if (result.success) {
        setIsFormOpen(false);
        toast.success(t('common.createSuccess'));
      } else {
        toast.error(result.message || t('common.createError'));
      }
    } catch (err) {
      toast.error(t('common.createError'));
    }
  };

  const handleUpdate = async (data: ProductFormData) => {
    if (!editingProduct) return;
    try {
      const result = await updateMutation.mutateAsync({
        code: editingProduct.id,
        data: {
          name: data.name,
          description: data.description,
          price: data.price,
          categoryCode: data.categoryId,
          stockQuantity: data.stock,
          color: data.color,
          power: data.power,
          voltage: data.voltage,
          material: data.material,
          size: data.size,
        },
      });
      if (result.success) {
        setIsFormOpen(false);
        setEditingProduct(null);
        toast.success(t('common.updateSuccess'));
      } else {
        toast.error(result.message || t('common.updateError'));
      }
    } catch (err) {
      toast.error(t('common.updateError'));
    }
  };

  const handleDelete = async () => {
    if (productToDelete) {
      try {
        await deleteMutation.mutateAsync(productToDelete.id);
        setProductToDelete(null);
        toast.success(t('common.deleteSuccess'));
      } catch (err) {
        toast.error(t('common.deleteError'));
      }
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.products')}</h1>
      </div>

      <DataTable
        columns={columns}
        data={products}
        keyField="id"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}

        // Sorting
        externalSortField={sortField}
        externalSortDir={sortDir}
        onExternalSort={handleSort}

        // Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setCurrentPage}

        // Actions
        onAdd={() => { setEditingProduct(null); setIsFormOpen(true); }}
        onEdit={(product) => { setEditingProduct(product); setIsFormOpen(true); }}
        onDelete={(product) => setProductToDelete(product)}

        // Search & Filters
        onAdvancedSearch={handleAdvancedSearch}
        advancedFilterDefs={[
          {
            id: 'name',
            label: t('admin.columns.name'),
            type: 'text',
            placeholder: t('admin.placeholders.searchProduct') || 'Tên sản phẩm...'
          },
          {
            id: 'category',
            label: t('admin.columns.category'),
            type: 'text',
            placeholder: t('admin.columns.category')
          },
          {
            id: 'price',
            label: t('admin.columns.price'),
            type: 'number',
            placeholder: 'Giá từ...'
          },
          {
            id: 'stock',
            label: t('admin.columns.stock'),
            type: 'number',
            placeholder: 'Tồn kho từ...'
          },
          {
            id: 'rating',
            label: t('admin.columns.rating'),
            type: 'number',
            placeholder: 'Đánh giá từ...'
          },
          {
            id: 'status',
            label: t('admin.columns.status'),
            type: 'select',
            options: [
              { value: 'active', label: t('common.status.active') },
              { value: 'inactive', label: t('common.status.inactive') }
            ]
          }
        ]}
        exportFilename="products_export"
        enableColumnVisibility={true}
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingProduct ? t('admin.actions.edit') : t('admin.actions.create')}
        size="lg"
      >
        <ProductForm
          initialData={editingProduct ? {
            name: editingProduct.name,
            description: editingProduct.description,
            price: editingProduct.price,
            categoryId: editingProduct.categoryId,
            stock: editingProduct.stock,
            image: editingProduct.image,
            color: editingProduct.color,
            power: editingProduct.power,
            voltage: editingProduct.voltage,
            material: editingProduct.material,
            size: editingProduct.size,
          } : undefined}
          onSubmit={editingProduct ? handleUpdate : handleCreate}
          onCancel={() => setIsFormOpen(false)}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!productToDelete}
        onClose={() => setProductToDelete(null)}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: 1 })}
        confirmText={t('common.delete')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default ProductsPage;
