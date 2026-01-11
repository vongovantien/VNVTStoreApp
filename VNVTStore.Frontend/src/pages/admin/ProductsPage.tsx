import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Eye, Star } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { ProductForm, ProductFormData } from './forms/ProductForm';
import {
  useProducts,
  useEntityManager,
} from '@/hooks';
import { productService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';
import { useToast } from '@/store';
import type { Product } from '@/types';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';

// Types for sorting
type SortField = 'name' | 'price' | 'stock' | 'createdAt';

export const ProductsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});
  const pageSize: number = PageSize.DEFAULT;

  // Sorting
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);

  // Fetch API
  const {
    data: productsData,
    isLoading,
    isFetching,
    isError,
    error,
    refetch,
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

  // Mutations and State via useEntityManager
  const {
    isFormOpen,
    editingItem: editingProduct,
    itemToDelete: productToDelete,
    isLoading: isSubmitting,
    isDeleting,
    openCreate,
    openEdit,
    closeForm,
    confirmDelete,
    cancelDelete,
    createMutation,
    updateMutation,
    deleteMutation
  } = useEntityManager<Product, CreateProductRequest, UpdateProductRequest>({
    service: productService as any,
    queryKey: ['products'],
  });

  // Selection State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const selectedToDelete = products.filter(p => selectedIds.has(p.id));
  const [showBulkConfirm, setShowBulkConfirm] = useState(false);

  // View State
  const [viewingProduct, setViewingProduct] = useState<Product | null>(null);
  const handleOpenView = (product: Product) => setViewingProduct(product);

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
          color={product.isActive !== false ? 'success' : 'error'}
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
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            title={t('admin.actions.view')}
            onClick={() => handleOpenView(product)}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            title="Sửa"
            onClick={() => {
              openEdit(product);
            }}
          >
            <Edit2 size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            title="Xóa"
            onClick={() => confirmDelete(product)}
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
    setSortDir(dir as SortDirection);
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
  };

  const handleAdvancedSearch = (filters: Record<string, string>) => {
    setAdvancedFilters(filters);
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
    // Sync text search
    setSearchQuery(filters.search || '');
  };

  const handleReset = () => {
    setAdvancedFilters({});
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
    setSearchQuery('');
    setSortField('createdAt');
    setSortDir(SortDirection.DESC);
    refetch(); // Force API refetch
  };

  const handleCreate = async (data: ProductFormData) => {
    try {
      await createMutation.mutateAsync({
        name: data.name,
        description: data.description,
        price: data.price,
        categoryCode: data.categoryId,
        stock: data.stock,
        color: data.color,
        power: data.power,
        voltage: data.voltage,
        material: data.material,
        size: data.size,
      });
      // Success handled by hook, but invalidating queries etc.
      // Hook handles toast and invalidation.
      // We might need to map data correctly.
      // The hook's createMutation expects CreateDto.
    } catch (err) {
      // Error handled by hook
    }
  };

  const handleUpdate = async (data: ProductFormData) => {
    if (!editingProduct) return;
    try {
      await updateMutation.mutateAsync({
        id: editingProduct.id,
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
    } catch (err) {
      // Error handled by hook
    }
  };

  const handleDelete = async () => {
    if (productToDelete) {
      deleteMutation.mutate(productToDelete.id);
    } else if (selectedToDelete.length > 0) {
      try {
        await Promise.all(selectedToDelete.map(item => deleteMutation.mutateAsync(item.id)));
        setSelectedIds(new Set()); // Clear selection
        setShowBulkConfirm(false);
        toast.success(t('common.deleteSuccess'));
      } catch (err) {
        // Errors handled by mutation individually
      }
    }
  };

  // Override cancelDelete to clear local state too
  const handleCancelDelete = () => {
    cancelDelete(); // from hook
    setSelectedIds(new Set());
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

        onAdd={() => { openCreate(); }}
        onView={handleOpenView}
        onEdit={(product) => { openEdit(product); }}
        onDelete={(product) => confirmDelete(product)}
        onBulkDelete={() => setShowBulkConfirm(true)}
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}

        // Search & Filters
        onAdvancedSearch={handleAdvancedSearch}
        onReset={handleReset}
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
        onClose={closeForm}
        title={editingProduct ? t('admin.actions.edit') : t('admin.actions.create')}
        size="3xl"
      >
        <ProductForm
          initialData={editingProduct ? {
            name: editingProduct.name,
            description: editingProduct.description,
            price: editingProduct.price,
            categoryId: editingProduct.categoryId,
            stock: editingProduct.stock,
            image: editingProduct.images?.[0], // adapt
            color: editingProduct.color,
            power: editingProduct.power,
            voltage: editingProduct.voltage,
            material: editingProduct.material,
            size: editingProduct.size,
          } : undefined}
          onSubmit={editingProduct ? handleUpdate : handleCreate}
          onCancel={closeForm}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      </Modal>

      {/* View Modal */}
      <Modal
        isOpen={!!viewingProduct}
        onClose={() => setViewingProduct(null)}
        title={t('admin.actions.view') + ' ' + t('admin.products')}
        size="lg"
      >
        {viewingProduct && (
          <div className="space-y-6">
            <div className="flex items-start gap-6">
              <img
                src={viewingProduct.images?.[0] || 'https://placehold.co/200'}
                alt={viewingProduct.name}
                className="w-32 h-32 rounded-lg object-cover border bg-white"
              />
              <div className="flex-1">
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{viewingProduct.name}</h2>
                <div className="flex items-center gap-2 mt-2">
                  <Badge color="info">{viewingProduct.brand || 'No Brand'}</Badge>
                  <Badge color="secondary">{viewingProduct.category || '-'}</Badge>
                  <Badge color={viewingProduct.isActive !== false ? 'success' : 'error'}>
                    {viewingProduct.isActive !== false ? t('common.status.active') : t('common.status.inactive')}
                  </Badge>
                </div>
                <div className="mt-4 flex items-baseline gap-2">
                  <span className="text-2xl font-bold text-rose-600">{formatCurrency(viewingProduct.price)}</span>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('admin.columns.info')}</h3>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between py-1 border-b border-dashed">
                    <span className="text-slate-500">{t('admin.columns.stock')}</span>
                    <span className="font-medium">{viewingProduct.stock}</span>
                  </div>
                  {viewingProduct.material && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('admin.columns.material')}</span>
                      <span className="font-medium">{viewingProduct.material}</span>
                    </div>
                  )}
                  {viewingProduct.size && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('admin.columns.size')}</span>
                      <span className="font-medium">{viewingProduct.size}</span>
                    </div>
                  )}
                  {viewingProduct.color && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('admin.columns.color')}</span>
                      <span className="font-medium">{viewingProduct.color}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('admin.columns.specs')}</h3>
                <div className="space-y-2 text-sm">
                  {viewingProduct.voltage && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('admin.columns.voltage')}</span>
                      <span className="font-medium">{viewingProduct.voltage}</span>
                    </div>
                  )}
                  {viewingProduct.power && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('admin.columns.power')}</span>
                      <span className="font-medium">{viewingProduct.power}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {viewingProduct.description && (
              <div>
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2 mb-2">{t('admin.columns.description')}</h3>
                <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed max-h-40 overflow-y-auto">
                  {viewingProduct.description}
                </p>
              </div>
            )}

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingProduct(null)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation */}
      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!productToDelete || selectedToDelete.length > 0}
        onClose={handleCancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: selectedToDelete.length > 0 ? selectedToDelete.length : 1 })}
        confirmText={t('common.delete')}
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};

export default ProductsPage;
