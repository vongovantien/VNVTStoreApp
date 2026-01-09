import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Eye, Star, Loader2, AlertCircle, ChevronUp, ChevronDown } from 'lucide-react';
import { Button, Badge, Modal, Pagination, ConfirmDialog } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { ProductForm, ProductFormData } from './forms/ProductForm';
import { TableToolbar } from './components/TableToolbar';
import { exportToCSV } from '@/utils/export';
import {
  useProducts,
  useCreateProduct,
  useUpdateProduct,
  useDeleteProduct,
} from '@/hooks';
import { useToast } from '@/store';
import type { Product } from '@/types';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { ColumnVisibility } from '@/components/admin/ColumnVisibility';

// Types for sorting
type SortField = 'name' | 'price' | 'stock' | 'createdAt';
type SortDirection = 'asc' | 'desc';

export const ProductsPage = () => {
  const { t } = useTranslation();

  // Pagination and Search State
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchField, setSearchField] = useState<string>('all');
  const pageSize: number = 10;

  // Sorting State
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>('desc');

  // Fetch products from API with sorting
  const {
    data: productsData,
    isLoading,
    isError,
    error,
    isFetching,
  } = useProducts({
    pageIndex: currentPage,
    pageSize,
    search: searchQuery || undefined,
    sortField,
    sortDir,
  });

  // Mutations
  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();
  const deleteMutation = useDeleteProduct();
  const toast = useToast();

  const products: Product[] = productsData?.products || [];
  const totalPages: number = productsData?.totalPages || 1;
  const totalItems: number = productsData?.totalItems || 0;

  // Selection State (for bulk operations)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [productToDelete, setProductToDelete] = useState<Product | null>(null);

  // Toolbar & Column Visibility State
  const [showSearch, setShowSearch] = useState(true);
  const [visibleColumns, setVisibleColumns] = useState<string[]>([
    'name', 'category', 'price', 'stock', 'rating', 'status', 'actions'
  ]);
  
  const columnsDef = [
    { id: 'name', label: t('admin.columns.name') },
    { id: 'category', label: t('admin.columns.category') },
    { id: 'price', label: t('admin.columns.price') },
    { id: 'stock', label: t('admin.columns.stock') },
    { id: 'rating', label: t('admin.columns.rating') },
    { id: 'status', label: t('admin.columns.status') },
    { id: 'actions', label: t('admin.columns.action') },
  ];

  // Sorting Handler
  const handleSort = useCallback((field: SortField): void => {
    if (sortField === field) {
      // Toggle direction if same field
      setSortDir(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      // New field, default to ascending
      setSortField(field);
      setSortDir('asc');
    }
    setCurrentPage(1); // Reset to first page on sort
  }, [sortField]);

  // Sort Icon Component
  const SortIcon = ({ field }: { field: SortField }): JSX.Element | null => {
    if (sortField !== field) return null;
    return sortDir === 'asc' 
      ? <ChevronUp size={14} className="inline ml-1" />
      : <ChevronDown size={14} className="inline ml-1" />;
  };

  // Selection Handlers
  const handleSelectAll = useCallback(
    (checked: boolean): void => {
      if (checked) {
        setSelectedIds(new Set(products.map((p) => p.id)));
      } else {
        setSelectedIds(new Set());
      }
    },
    [products]
  );

  const handleSelectRow = useCallback((id: string): void => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  // Handlers
  const handleExport = (): void => {
    exportToCSV(products, 'products_export');
  };

  const handleBulkDelete = async (): Promise<void> => {
    if (confirm(t('common.confirmDelete', { count: selectedIds.size }))) {
      for (const id of selectedIds) {
        await deleteMutation.mutateAsync(id);
      }
      setSelectedIds(new Set());
    }
  };

  const handleAddProduct = async (data: ProductFormData): Promise<void> => {
    try {
      const result = await createMutation.mutateAsync({
        name: data.name,
        description: data.description,
        price: data.price,
        categoryCode: data.categoryId,
        stockQuantity: data.stock,
      });

      if (result.success) {
        setIsFormOpen(false);
        toast.success(t('common.createSuccess'));
      } else {
        toast.error(result.message || t('common.createError'));
      }
    } catch (err) {
      console.error('Failed to create product:', err);
      toast.error(t('common.createError'));
    }
  };

  const handleEditProduct = async (data: ProductFormData): Promise<void> => {
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
      console.error('Failed to update product:', err);
      toast.error(t('common.updateError'));
    }
  };

  const handleDeleteProduct = async (): Promise<void> => {
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


  const openEditModal = (product: Product): void => {
    setEditingProduct(product);
    setIsFormOpen(true);
  };

  // Error State - show as alert, not full page
  const showError = isError && !isLoading;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.products')}</h1>
        <Button leftIcon={<Plus size={20} />} onClick={() => { setEditingProduct(null); setIsFormOpen(true); }}>
          {t('admin.addProduct')}
        </Button>
      </div>



      <div className="flex flex-col gap-4">
        <div className="flex justify-between items-center gap-4 bg-white dark:bg-slate-800 p-2 rounded-lg border shadow-sm">
            <AdminToolbar 
                onAdd={() => { setEditingProduct(null); setIsFormOpen(true); }}
                onEdit={() => { 
                   if (selectedIds.size === 1) {
                      const id = Array.from(selectedIds)[0];
                      const product = products.find(p => p.id === id);
                      if (product) openEditModal(product);
                   }
                }}
                onDelete={handleBulkDelete}
                onSearchClick={() => setShowSearch(!showSearch)}
                onReset={() => {
                  setSearchQuery('');
                  setSearchField('all');
                  setSortField('createdAt');
                  setSortDir('desc');
                  setCurrentPage(1);
                  setSelectedIds(new Set());
                }}
                onExport={handleExport}
                isSearchActive={showSearch}
                selectedCount={selectedIds.size}
            />
            <ColumnVisibility 
                columns={columnsDef} 
                visibleColumns={visibleColumns} 
                onChange={setVisibleColumns} 
            />
        </div>

        {showSearch && (
          <TableToolbar
            searchQuery={searchQuery}
            onSearchChange={(val: string) => {
              setSearchQuery(val);
              setCurrentPage(1);
            }}
            searchField={searchField}
            onSearchFieldChange={setSearchField}
            searchOptions={[
              { label: 'Tên sản phẩm', value: 'name' },
              { label: 'Danh mục', value: 'category' },
              { label: 'Hãng', value: 'brand' }
            ]}
            selectedCount={selectedIds.size}
            onBulkDelete={handleBulkDelete}
            onExport={handleExport}
          />
        )}
      </div>

      {/* Table */}
      <div className="bg-primary rounded-xl overflow-hidden shadow-sm border relative min-h-[400px]">
        {/* Loading/Fetching overlay - show on initial load or refetch */}
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
              <p className="text-secondary text-sm">
                {error instanceof Error ? error.message : 'Không thể tải dữ liệu'}
              </p>
            </div>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full min-w-[1000px]">
            <thead className="bg-secondary border-b">
              <tr>
                <th className="px-4 py-3 w-[40px]">
                  <input
                    type="checkbox"
                    className="rounded border-gray-300"
                    onChange={(e) => handleSelectAll(e.target.checked)}
                    checked={products.length > 0 && selectedIds.size >= products.length}
                  />
                </th>
                {visibleColumns.includes('name') && (
                  <th 
                    className="px-4 py-3 text-left text-sm font-semibold cursor-pointer hover:bg-tertiary/10 transition-colors"
                    onClick={() => handleSort('name')}
                  >
                    {t('admin.columns.name')} <SortIcon field="name" />
                  </th>
                )}
                {visibleColumns.includes('category') && (
                  <th className="px-4 py-3 text-left text-sm font-semibold">{t('admin.columns.category')}</th>
                )}
                {visibleColumns.includes('price') && (
                  <th 
                    className="px-4 py-3 text-right text-sm font-semibold cursor-pointer hover:bg-tertiary/10 transition-colors"
                    onClick={() => handleSort('price')}
                  >
                    {t('admin.columns.price')} <SortIcon field="price" />
                  </th>
                )}
                {visibleColumns.includes('stock') && (
                  <th 
                    className="px-4 py-3 text-center text-sm font-semibold cursor-pointer hover:bg-tertiary/10 transition-colors"
                    onClick={() => handleSort('stock')}
                  >
                    {t('admin.columns.stock')} <SortIcon field="stock" />
                  </th>
                )}
                {visibleColumns.includes('rating') && (
                  <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.columns.rating')}</th>
                )}
                {visibleColumns.includes('status') && (
                  <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.columns.status')}</th>
                )}
                {visibleColumns.includes('actions') && (
                  <th className="px-4 py-3 text-right text-sm font-semibold">{t('admin.columns.action')}</th>
                )}
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={visibleColumns.length + 1} className="px-4 py-12 text-center text-secondary">
                    Không có sản phẩm nào
                  </td>
                </tr>
              ) : (
                products.map((product) => (
                  <tr key={product.id} className={`border-b last:border-0 hover:bg-secondary/50 transition-colors ${selectedIds.has(product.id) ? 'bg-accent/5' : ''}`}>

                    <td className="px-4 py-4">
                      <input
                        type="checkbox"
                        className="rounded border-gray-300"
                        checked={selectedIds.has(product.id)}
                        onChange={() => handleSelectRow(product.id)}
                      />
                    </td>
                    {visibleColumns.includes('name') && (
                      <td className="px-4 py-4">
                        <div className="flex items-center gap-3">
                          <img
                            src={product.images?.[0] || 'https://placehold.co/100?text=No+Image'}
                            alt={product.name}
                            className="w-12 h-12 rounded-lg object-cover border"
                          />
                          <div>
                            <p className="font-medium text-sm">{product.name}</p>
                            <p className="text-xs text-tertiary">{product.brand}</p>
                          </div>
                        </div>
                      </td>
                    )}
                    {visibleColumns.includes('category') && (
                      <td className="px-4 py-4 text-secondary">{product.category}</td>
                    )}
                    {visibleColumns.includes('price') && (
                      <td className="px-4 py-4 text-right">
                        {product.price > 0 ? (
                          <span className="font-semibold text-error">{formatCurrency(product.price)}</span>
                        ) : (
                          <Badge color="primary" size="sm">Liên hệ</Badge>
                        )}
                      </td>
                    )}
                    {visibleColumns.includes('stock') && (
                      <td className="px-4 py-4 text-center">
                        <span className={product.stock > 10 ? 'text-success' : product.stock > 0 ? 'text-warning' : 'text-error'}>
                          {product.stock}
                        </span>
                      </td>
                    )}
                    {visibleColumns.includes('rating') && (
                      <td className="px-4 py-4 text-center">
                        <div className="flex items-center justify-center gap-1 text-sm">
                          <Star size={16} className="text-warning fill-warning" />
                          <span>{product.rating} ({product.reviewCount})</span>
                        </div>
                      </td>
                    )}
                    {visibleColumns.includes('status') && (
                      <td className="px-4 py-4 text-center">
                        <Badge 
                          color={product.isActive !== false ? 'success' : 'secondary'} 
                          size="sm"
                          variant="outline"
                        >
                          {product.isActive !== false ? t('common.status.active') : t('common.status.inactive')}
                        </Badge>
                      </td>
                    )}
                    {visibleColumns.includes('actions') && (
                      <td className="px-4 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button className="p-2 hover:bg-secondary rounded-lg transition-colors" title="Xem">
                            <Eye size={16} className="text-secondary" />
                          </button>
                          <button
                            className="p-2 hover:bg-secondary rounded-lg transition-colors"
                            title="Sửa"
                            onClick={() => openEditModal(product)}
                          >
                            <Edit2 size={16} className="text-primary" />
                          </button>
                          <button
                            className="p-2 hover:bg-error/10 rounded-lg transition-colors"
                            title="Xóa"
                            onClick={() => setProductToDelete(product)}
                          >
                            <Trash2 size={16} className="text-error" />
                          </button>
                        </div>
                      </td>
                    )}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalItems={totalItems}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
        />
      </div>

      {/* Product Form Modal */}
      {/* Product Form Modal */}
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
          } : undefined}
          onSubmit={editingProduct ? handleEditProduct : handleAddProduct}
          onCancel={() => setIsFormOpen(false)}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      </Modal>


      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        isOpen={!!productToDelete}
        onClose={() => setProductToDelete(null)}
        onConfirm={handleDeleteProduct}
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
