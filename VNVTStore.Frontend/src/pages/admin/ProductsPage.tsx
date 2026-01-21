import { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Edit2, Trash2, Eye, Star, Plus, MoreHorizontal, Edit3, Copy, Archive, FolderInput, Share2, Heart, Package } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { Dropdown, DropdownItem } from '@/components/ui/Dropdown';
import { formatCurrency, getImageUrl } from '@/utils/format';
import { ProductForm, ProductFormData } from './forms/ProductForm';
import {
  useProducts,
  useEntityManager,
} from '@/hooks';
import { productService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';
import { useToast } from '@/store';
import type { Product } from '@/types';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';
import { PRODUCT_LIST_FIELDS } from '@/constants/fieldConstants';

// Types for sorting
type SortField = 'name' | 'price' | 'stock' | 'createdAt';

export const ProductsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState<number>(PaginationDefaults.PAGE_SIZE);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Sorting
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);

  // Import
  const handleImportProduct = async (file: File) => {
    try {
        await productService.import(file);
        toast.success(t('common.messages.importSuccess') || 'Import successful');
        refetch();
    } catch (error) {
        toast.error(t('common.messages.importError') || 'Import failed');
        throw error; // Let modal handle error state if needed
    }
  };

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
    fields: PRODUCT_LIST_FIELDS,  // Selective columns for list view
    ...advancedFilters
  });

  const products: Product[] = productsData?.products || [];
  const totalItems: number = productsData?.totalItems || 0;
  const totalPages: number = Math.ceil(totalItems / pageSize) || 1;

  // Mutations and State via useEntityManager
  const {
    isFormOpen,
    editingItem: editingProduct,
    itemToDelete: productToDelete,
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
  const selectedToDelete = products.filter(p => selectedIds.has(p.code));
  const [showBulkConfirm, setShowBulkConfirm] = useState(false);

  // View State
  const [viewingProduct, setViewingProduct] = useState<Product | null>(null);
  const handleOpenView = (product: Product) => setViewingProduct(product);

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
    setPageSize(PaginationDefaults.PAGE_SIZE);
    setSearchQuery('');
    setSortField('createdAt');
    setSortDir(SortDirection.DESC);
  };

  const handleCreate = async (data: ProductFormData) => {
    try {
      await createMutation.mutateAsync({
        name: data.name,
        description: data.description,
        price: data.price,
        categoryCode: data.categoryId,
        stock: data.stock,
        costPrice: data.costPrice,

        weight: data.weight,
        supplierCode: data.supplierCode,
        brand: data.brand,
        color: data.color,
        power: data.power,
        voltage: data.voltage,
        material: data.material,
        size: data.size,
        images: data.images,
        isActive: data.isActive,
      });
    } catch (err) {
      // Error handled by hook
    }
  };

  const handleUpdate = async (data: ProductFormData) => {
    if (!editingProduct) return;
    try {
      await updateMutation.mutateAsync({
        id: editingProduct.code, 
        data: {
          name: data.name,
          description: data.description,
          price: data.price,
          categoryCode: data.categoryId,
          stockQuantity: data.stock,
          costPrice: data.costPrice,

          weight: data.weight,
          supplierCode: data.supplierCode,
          brand: data.brand,
          color: data.color,
          power: data.power,
          voltage: data.voltage,
          material: data.material,
          size: data.size,
          images: data.images,
          isActive: data.isActive,
        },
      });
    } catch (err) {
      // Error handled by hook
    }
  };

  const handleDelete = async () => {
    if (productToDelete) {
      deleteMutation.mutate(productToDelete.code);
    } else if (selectedToDelete.length > 0) {
      try {
        await Promise.all(selectedToDelete.map(item => deleteMutation.mutateAsync(item.code)));
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
    setShowBulkConfirm(false);
    // Don't clear selection on cancel - user may want to keep their selection
  };

  // Column Definitions
  const columns: DataTableColumn<Product>[] = [
    {
      id: 'image',
      header: t('common.fields.image'),
      width: '80px',
      className: 'text-center',
      accessor: (product) => (
        <div className="flex flex-col items-center">
            <div className="w-10 h-10 rounded-lg overflow-hidden bg-gray-100 dark:bg-slate-700 border border-gray-200 dark:border-slate-600">
               {product.images?.[0] ? (
                 <img 
                   src={getImageUrl(product.images[0])} 
                   alt={product.name} 
                   className="w-full h-full object-cover"
                   onError={(e) => console.error("Img Error:", e.currentTarget.src)}
                 />
               ) : (
                 <div className="w-full h-full flex items-center justify-center text-gray-400">
                   <Package size={18} />
                 </div>
               )}
            </div>
        </div>
      )
    },
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: (product) => (
        <div>
          <p className="font-medium text-sm text-slate-700 dark:text-slate-200">{product.name}</p>
          <p className="text-xs text-slate-500">{product.brand}</p>
        </div>
      ),
      sortable: true
    },
    {
      id: 'category',
      header: t('common.fields.category'),
      accessor: 'category',
    },
    {
      id: 'price',
      header: t('common.fields.price'),
      accessor: (product) => (
        product.price > 0 ? (
          <span className="font-semibold text-rose-600">{formatCurrency(product.price)}</span>
        ) : (
          <Badge color="primary" size="sm">{t('common.contact')}</Badge>
        )
      ),
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'stock',
      header: t('common.fields.stock'),
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
      header: t('common.fields.rating'),
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
      header: t('common.fields.status'),
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
      header: t('common.fields.action'),
      accessor: (product) => (
        <TableActions
          onView={() => handleOpenView(product)}
          onEdit={() => openEdit(product)}
          onDelete={() => confirmDelete(product)}
        />
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      width: '100px'
    }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="common.modules.products"
        subtitle="admin.subtitles.products"
      />

      <DataTable
        columns={columns}
        data={products}
        keyField="code"
        enableSelection
        isLoading={isLoading}
        isFetching={isFetching}
        onAdd={() => openCreate()}
        onRefresh={() => refetch()}
        onImport={handleImportProduct}
        importTemplateUrl="/products/template"
        importTitle={t('common.importData')}
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
        onPageSizeChange={(size) => {
          setPageSize(size);
          setCurrentPage(PaginationDefaults.PAGE_INDEX);
        }}
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
            label: t('common.fields.name'),
            type: 'text',
            placeholder: t('common.placeholders.searchProduct')
          },
          {
            id: 'category',
            label: t('common.fields.category'),
            type: 'text',
            placeholder: t('common.fields.category')
          },
          {
            id: 'price',
            label: t('common.fields.price'),
            type: 'number',
            placeholder: t('common.placeholders.priceFrom')
          },
          {
            id: 'stock',
            label: t('common.fields.stock'),
            type: 'number',
            placeholder: t('common.placeholders.stockFrom')
          },
          {
            id: 'rating',
            label: t('common.fields.rating'),
            type: 'number',
            placeholder: t('common.placeholders.ratingFrom')
          },
          {
            id: 'status',
            label: t('common.fields.status'),
            type: 'select',
            options: [
              { value: 'active', label: t('common.status.active') },
              { value: 'inactive', label: t('common.status.inactive') }
            ]
          }
        ]}
        exportFilename="products_export"
        exportColumns={[
          { key: 'code', label: t('common.fields.code'), width: 15 },
          { key: 'name', label: t('common.fields.name'), width: 30 },
          { key: 'category', label: t('common.fields.category'), width: 20 },
          { key: 'price', label: t('common.fields.price'), width: 15 },
          { key: 'stock', label: t('common.fields.stock'), width: 12 },
          { key: 'color', label: t('common.fields.color'), width: 12 },
          { key: 'material', label: t('common.fields.material'), width: 15 },
          { key: 'power', label: t('common.fields.power'), width: 12 },
          { key: 'voltage', label: t('common.fields.voltage'), width: 12 },
          { key: 'size', label: t('common.fields.size'), width: 15 },
          { key: 'isActive', label: t('common.fields.status'), width: 12 },
        ]}
        onExportAllData={async () => {
          const response = await productService.search({ pageIndex: 1, pageSize: 10000 });
          return (response.data?.items || []) as unknown as Product[];
        }}
        enableColumnVisibility={true}
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingProduct ? t('common.actions.edit') : t('common.actions.create')}
        size="3xl"
        // Force unmount on close to reset form state
        // key={isFormOpen ? 'open' : 'closed'} 
      >
        <ProductForm
          initialData={editingProduct ? {
            name: editingProduct.name,
            description: editingProduct.description,
            price: editingProduct.price,
            categoryId: editingProduct.categoryId,
            stock: editingProduct.stock,
            images: editingProduct.images || [],
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
        title={t('common.actions.view') + ' ' + t('common.modules.products')}
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
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('common.fields.info')}</h3>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between py-1 border-b border-dashed">
                    <span className="text-slate-500">{t('common.fields.stock')}</span>
                    <span className="font-medium">{viewingProduct.stock}</span>
                  </div>
                  {viewingProduct.material && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.material')}</span>
                      <span className="font-medium">{viewingProduct.material}</span>
                    </div>
                  )}
                  {viewingProduct.size && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.size')}</span>
                      <span className="font-medium">{viewingProduct.size}</span>
                    </div>
                  )}
                  {viewingProduct.color && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.color')}</span>
                      <span className="font-medium">{viewingProduct.color}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('common.fields.specs')}</h3>
                <div className="space-y-2 text-sm">
                  {viewingProduct.voltage && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.voltage')}</span>
                      <span className="font-medium">{viewingProduct.voltage}</span>
                    </div>
                  )}
                  {viewingProduct.power && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.power')}</span>
                      <span className="font-medium">{viewingProduct.power}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {viewingProduct.description && (
              <div>
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2 mb-2">{t('common.fields.description')}</h3>
                <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed max-h-40 overflow-y-auto">
                  {viewingProduct.description}
                </p>
              </div>
            )}

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingProduct(null)}>
                {t('common.actions.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!productToDelete || showBulkConfirm}
        onClose={handleCancelDelete}
        onConfirm={handleDelete}
        title={t('common.actions.delete')}
        message={t('common.confirmDelete', { count: selectedToDelete.length > 0 ? selectedToDelete.length : 1 })}
        confirmText={t('common.actions.delete')}
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};

export default ProductsPage;
