import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Package, AlertTriangle, XCircle } from 'lucide-react';
import { Modal, ConfirmDialog } from '@/components/ui';


import { ProductForm, ProductFormData } from './forms/ProductForm';
import {
  useProducts,
  useEntityManager,
  type EntityService
} from '@/hooks';
import { useQuery, useMutation } from '@tanstack/react-query';
import { productService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';
import { useToast } from '@/store';
import type { Product, ProductUnit } from '@/types';
import { DataTable } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { PaginationDefaults, SortDirection } from '@/constants';
import { PRODUCT_LIST_FIELDS } from '@/constants/fieldConstants';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';

import { ProductDetailModal } from './components/ProductDetailModal';
import { useProductColumns } from './hooks/useProductColumns';

// Types for sorting
type SortField = 'name' | 'price' | 'stock' | 'createdAt';

export const ProductsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();
  
  // Get columns from hook
  const columns = useProductColumns();


  // Helper to extract detail/spec value
  const getDetailValue = (details: { specName: string; specValue: string }[] | undefined, keys: string[]) => {
      if (!details) return undefined;
      const detail = details.find(d => keys.some(k => d.specName?.toLowerCase() === k.toLowerCase()));
      return detail?.specValue;
  };

  // State
  const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState<number>(PaginationDefaults.PAGE_SIZE);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Sorting
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);

  // Import Mutation
  const importMutation = useMutation({
    mutationFn: (file: File) => productService.import(file),
    onSuccess: () => {
      toast.success(t('common.messages.importSuccess') || 'Import successful');
      refetch();
    },
    onError: (error: Error) => {
      toast.error(error.message || t('common.messages.importError') || 'Import failed');
    },
  });

  // Handle Import (now uses the mutation)
  const handleImportProduct = async (file: File) => {
    await importMutation.mutateAsync(file);
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
    ...(searchQuery ? { search: searchQuery } : {}),
    sortField,
    sortDir,
    ...(PRODUCT_LIST_FIELDS.length > 0 ? { fields: PRODUCT_LIST_FIELDS } : {}),  // Selective columns for list view
    ...advancedFilters
  });

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['products', 'stats'],
      queryFn: () => productService.getStats(),
      staleTime: 60000,
  });

  const stats: StatItem[] = [
    {
      label: t('admin.stats.totalProducts'),
      value: statsData?.total || 0,
      icon: <Package size={24} />,
      color: 'blue',
      loading: isStatsLoading
    },
    {
      label: t('admin.stats.lowStock'),
      value: statsData?.lowStock || 0,
      icon: <AlertTriangle size={24} />,
      color: 'amber',
      loading: isStatsLoading
    },
    {
      label: t('admin.stats.outOfStock'),
      value: statsData?.outOfStock || 0,
      icon: <XCircle size={24} />,
      color: 'rose',
      loading: isStatsLoading
    }
  ];

  const products: Product[] = productsData?.products || [];
  const totalItems: number = productsData?.totalItems || 0;
  const totalPages: number = Math.ceil(totalItems / pageSize) || 1;

  // Mutations and State via useEntityManager
  const {
    isFormOpen,
    editingItem: editingProduct,
    itemToDelete: productToDelete,
    openCreate,
    openEdit,
    closeForm,
    confirmDelete,
    cancelDelete,
    createMutation,
    updateMutation,
    deleteMutation
  } = useEntityManager<Product, CreateProductRequest, UpdateProductRequest>({
    service: productService as unknown as EntityService<Product, CreateProductRequest, UpdateProductRequest>,
    queryKey: ['products'],
    includeChildrenOnEdit: true,
  });

  // Selection State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [itemsToDelete, setItemsToDelete] = useState<Product[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => productService.deleteMultiple(codes),
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

  const handleBulkDelete = (items: Product[]) => {
    setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
      bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

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
        ...(data.description ? { description: data.description } : {}),
        price: data.price,
        categoryCode: data.categoryCode,
        stockQuantity: data.stockQuantity,
        ...(data.costPrice !== undefined ? { costPrice: data.costPrice } : {}),

        ...(data.weight !== undefined ? { weight: data.weight } : {}),
        ...(data.supplierCode ? { supplierCode: data.supplierCode } : {}),
        ...(data.brandCode ? { brandCode: data.brandCode } : {}),
        color: data.color,
        power: data.power,
        voltage: data.voltage,
        material: data.material,
        size: data.size,
        images: data.images,
        isActive: data.isActive,
        details: data.details,
        baseUnit: data.baseUnit,
        minStockLevel: data.minStockLevel,
        binLocation: data.binLocation,
        vatRate: data.vatRate,
        countryOfOrigin: data.countryOfOrigin,
        productUnits: (data.productUnits as ProductUnit[])?.map(u => ({
            ...u,
            isBaseUnit: u.isBaseUnit || false
        })) || [],
      });
    } catch {
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
          ...(data.description ? { description: data.description } : {}),
          price: data.price,
          categoryCode: data.categoryCode,
          stockQuantity: data.stockQuantity,
          ...(data.costPrice !== undefined ? { costPrice: data.costPrice } : {}),

          ...(data.weight !== undefined ? { weight: data.weight } : {}),
          ...(data.supplierCode ? { supplierCode: data.supplierCode } : {}),
          ...(data.brandCode ? { brandCode: data.brandCode } : {}),
          color: data.color,
          power: data.power,
          voltage: data.voltage,
          material: data.material,
          size: data.size,
          images: data.images,
          isActive: data.isActive,
          details: data.details,
          baseUnit: data.baseUnit,
          minStockLevel: data.minStockLevel,
          binLocation: data.binLocation,
          vatRate: data.vatRate,
          countryOfOrigin: data.countryOfOrigin,
          productUnits: (data.productUnits as ProductUnit[])?.map(u => ({
            ...u,
            isBaseUnit: u.isBaseUnit || false
          })) || [],
        },
      });
    } catch {
      // Error handled by hook
    }
  };

  const handleDelete = async () => {
    if (productToDelete) {
      deleteMutation.mutate(productToDelete.code);
    }
  };




      const productInitialData = useMemo(() => editingProduct ? {
        name: editingProduct.name,
        description: editingProduct.description,
        price: editingProduct.price,
        categoryCode: editingProduct.categoryCode,
        stockQuantity: editingProduct.stockQuantity ?? editingProduct.stock,
        images: editingProduct.images || [],
        productImages: editingProduct.productImages,
        details: editingProduct.details,
        color: editingProduct.color || getDetailValue(editingProduct.details, ['color', 'màu', 'màu sắc']),
        power: editingProduct.power || getDetailValue(editingProduct.details, ['power', 'công suất']),
        voltage: editingProduct.voltage || getDetailValue(editingProduct.details, ['voltage', 'điện áp']),
        material: editingProduct.material || getDetailValue(editingProduct.details, ['material', 'chất liệu']),
        size: editingProduct.size || getDetailValue(editingProduct.details, ['size', 'kích thước']),
        weight: editingProduct.weight || (getDetailValue(editingProduct.details, ['weight', 'khối lượng']) ? Number(getDetailValue(editingProduct.details, ['weight', 'khối lượng'])) : undefined),
        supplierCode: editingProduct.supplierCode,
        brandCode: editingProduct.brandCode,
        isActive: editingProduct.isActive,
        categoryName: editingProduct.category,
        brandName: editingProduct.brand,
        supplierName: editingProduct.supplierName,
        baseUnit: editingProduct.baseUnit,
        vatRate: editingProduct.vatRate,
        minStockLevel: editingProduct.minStockLevel,
        binLocation: editingProduct.binLocation,
        countryOfOrigin: editingProduct.countryOfOrigin || getDetailValue(editingProduct.details, ['origin', 'xuất xứ', 'nước sản xuất']),
        productUnits: editingProduct.productUnits,
      } : undefined, [editingProduct]);

      return (
        <div className="space-y-6">
          <AdminPageHeader
            title="common.modules.products"
            subtitle="admin.subtitles.products"
          />

          <StatsCards stats={stats} />

          <DataTable
            columns={columns}
            data={products}
            keyField="code"
            enableSelection
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onBulkDelete={handleBulkDelete}
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

            // Search & Filters
            onSearch={setSearchQuery}
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
            size="7xl"
            // Force unmount on close to reset form state
          >
            <ProductForm
              initialData={productInitialData as unknown as ProductFormData}
              onSubmit={editingProduct ? handleUpdate : handleCreate}
              onCancel={closeForm}
              isLoading={createMutation.isPending || updateMutation.isPending}
            />
          </Modal>

      {/* View Modal */}
      <ProductDetailModal 
        product={viewingProduct} 
        onClose={() => setViewingProduct(null)} 
      />
      
      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!productToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete', { name: productToDelete?.name })}
        confirmText={t('admin.actions.delete')}
        cancelText={t('common.cancel')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />

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
};

export default ProductsPage;
