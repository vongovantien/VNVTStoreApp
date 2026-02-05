
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Tag } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { brandService, productService, type BrandDto, type CreateBrandRequest, type UpdateBrandRequest } from '@/services';
import { DataTable, type DataTableColumn } from '@/components/common';
import { AdminPageHeader } from '@/components/admin';
import { useEntityManager, useBrands } from '@/hooks';
import { BrandForm, type BrandFormData } from './forms';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';
import { getImageUrl } from '@/utils/format';
import { SearchCondition } from '@/services/baseService';
import { useMemo } from 'react';

export default function BrandsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [filters, setFilters] = useState<Record<string, string>>({});

  // Data Fetching
  const searchParams = useMemo(() => {
       const filterParams = [];
       if (filters.Name) filterParams.push({ field: 'Name', value: filters.Name, operator: SearchCondition.Contains });
       if (filters.IsActive) filterParams.push({ field: 'IsActive', value: filters.IsActive === 'true', operator: SearchCondition.Equal });
       
       return {
          pageIndex: page,
          pageSize,
          filters: filterParams
       };
    }, [page, pageSize, filters]);

  const { 
    data: result, 
    isLoading, 
    refetch 
  } = useBrands(searchParams);

  const brands = result?.data?.items || [];
  const totalItems = result?.data?.totalItems || 0;
  const totalPages = Math.ceil(totalItems / pageSize);

  // Entity Manager
  const {
      isFormOpen,
      editingItem: editingBrand,
      itemToDelete: brandToDelete,
      isDeleting,
      openCreate,
      openEdit,
      closeForm,
      confirmDelete: baseConfirmDelete,
      cancelDelete,
      createMutation,
      updateMutation,
      deleteMutation
  } = useEntityManager<BrandDto, CreateBrandRequest, UpdateBrandRequest>({
      service: brandService,
      queryKey: ['brands']
  });

  const confirmDelete = async (item: BrandDto) => {
    try {
        const result = await productService.search({ filters: [{ field: 'BrandCode', value: item.code }], pageSize: 1 });
        if (result.success && result.data && result.data.totalItems > 0) {
             alert(t('admin.brands.cannotDelete', { count: result.data.totalItems, defaultValue: `Cannot delete: Brand contains ${result.data.totalItems} products.` }));
             return;
        }
        baseConfirmDelete(item);
    } catch (error) {
        console.error("Check dependency failed", error);
        baseConfirmDelete(item);
    }
  };

  const { mutate: createBrand, isPending: isCreating } = createMutation;
  const { mutate: updateBrand, isPending: isUpdating } = updateMutation;
  const { mutate: deleteBrand } = deleteMutation;

  // View State
  const [viewingBrand, setViewingBrand] = useState<BrandDto | null>(null);

  // Selection
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Bulk Delete State
  const [itemsToDelete, setItemsToDelete] = useState<BrandDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => brandService.deleteMultiple(codes),
    onSuccess: () => {
      setItemsToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['brands'] });
    }
  });

  const handleBulkDelete = (items: BrandDto[]) => {
    setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
      bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

  const handleFormSubmit = async (formData: BrandFormData) => {
    if (editingBrand) {
      await updateBrand({
          id: editingBrand.code, 
          data: formData as UpdateBrandRequest
      }, {
          onSuccess: () => closeForm()
      });
    } else {
      await createBrand(formData as unknown as CreateBrandRequest, {
          onSuccess: () => closeForm()
      });
    }
  };

  const handleDelete = () => {
    if (brandToDelete) {
      deleteBrand(brandToDelete.code);
    }
  };

  const isSubmitting = isCreating || isUpdating;

  // Initial Data
  const prepareInitialData = (): Partial<BrandFormData> | undefined => {
      if (!editingBrand) return undefined;
      return {
          name: editingBrand.name,
          description: editingBrand.description || undefined,
          logoUrl: editingBrand.logoUrl || undefined,
          isActive: editingBrand.isActive
      };
  };

  // Columns
  const columns: DataTableColumn<BrandDto>[] = [
    {
      id: 'logo',
      header: t('common.fields.image'),
      accessor: (brand) => brand.logoUrl ? (
          <img src={getImageUrl(brand.logoUrl)} alt={brand.name} className="w-10 h-10 object-contain rounded-md border border-gray-100" />
      ) : (
          <div className="w-10 h-10 rounded-md bg-gray-50 flex items-center justify-center text-gray-400">
              <Tag size={20} />
          </div>
      ),
      className: 'w-[120px]'
    },
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: 'name',
      sortable: true,
    },
    {
      id: 'description',
      header: t('common.fields.description'),
      accessor: 'description',
      className: 'hidden md:table-cell max-w-[300px] truncate'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (brand) => (
        <Badge color={brand.isActive !== false ? 'success' : 'secondary'}>
          {brand.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
        </Badge>
      ),
      className: 'text-center w-[120px]',
    },
    {
        id: 'actions',
        header: '',
        className: 'w-[100px]',
        accessor: (brand) => (
          <TableActions
            onView={() => setViewingBrand(brand)}
            onEdit={() => openEdit(brand)}
            onDelete={() => confirmDelete(brand)}
          />
        )
      }
  ];

  // Stats
  // Using simplified stats based on loaded data or separate query if API supported
  const stats: StatItem[] = [
      {
          label: t('common.fields.brand'),
          value: totalItems, 
          icon: <Tag size={24} />,
          color: 'blue',
      },
      // Not accurate if paginated, but shows loaded count? Or keep fetching specific stats endpoint if exists
      // If server provides stats endpoint, use it. BaseService doesn't seem to have getStats by default.
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.brands" 
        subtitle="admin.subtitles.brands" 
      />

      <StatsCards stats={stats} />

      <DataTable
        data={brands}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder={t('common.placeholders.search')}
        
        // Server-side props
        currentPage={page}
        pageSize={pageSize}
        totalItems={totalItems}
        totalPages={totalPages}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onRefresh={() => refetch()}

        // Advanced Filters
        advancedFilterDefs={[
             { id: 'Name', label: t('common.fields.name'), type: 'text', placeholder: t('common.placeholders.search') },
             { id: 'IsActive', label: t('common.fields.status'), type: 'select', options: [{ value: 'true', label: t('admin.status.active') }, { value: 'false', label: t('admin.status.inactive') }] }
        ]}
        onAdvancedSearch={setFilters}

        // Export
        onExportAllData={async () => {
           const res = await brandService.getAll(10000);
           return res.data?.items || [];
        }}
        exportFilename="brands"

        keyField="code"
        enableSelection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onBulkDelete={handleBulkDelete}
        onAdd={() => openCreate()}
        onEdit={(item) => openEdit(item)}
        onDelete={(item) => confirmDelete(item)}
        onView={(item) => setViewingBrand(item)}
      />

       {/* Form Modal */}
       {isFormOpen && (
        <BrandForm
            initialData={prepareInitialData()}
            onSubmit={handleFormSubmit}
            onCancel={closeForm}
            isLoading={isSubmitting}
            modalOpen={isFormOpen}
            modalTitle={editingBrand ? t('admin.actions.edit') : t('admin.actions.create')}
            imageBaseUrl="https://example.com" 
        />
       )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!brandToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete', { name: brandToDelete?.name })}
        confirmText={t('admin.actions.delete')}
        cancelText={t('common.cancel')}
        variant="danger"
        isLoading={isDeleting}
      />

      {/* Bulk Delete Confirmation */}
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

      {/* View Details Modal */}
      {viewingBrand && (
        <Modal
          isOpen={!!viewingBrand}
          onClose={() => setViewingBrand(null)}
          title={t('admin.actions.viewDetails')}
          size="md"
        >
          <div className="space-y-6">
             <div className="flex items-center gap-4 border-b border-gray-100 pb-4">
                {viewingBrand.logoUrl ? (
                    <img src={getImageUrl(viewingBrand.logoUrl)} className="w-16 h-16 object-contain rounded-lg border border-gray-100" />
                ) : (
                    <div className="w-16 h-16 bg-gray-50 rounded-lg flex items-center justify-center">
                        <Tag className="text-gray-400" />
                    </div>
                )}
                <div>
                    <h2 className="text-xl font-bold">{viewingBrand.name}</h2>
                    <Badge color={viewingBrand.isActive !== false ? 'success' : 'secondary'} className="mt-1">
                        {viewingBrand.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
                    </Badge>
                </div>
             </div>
             
             <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.description')}</label>
                <p className="text-sm mt-1">{viewingBrand.description || t('common.none')}</p>
             </div>
             
             <div className="flex justify-end pt-4">
               <Button onClick={() => setViewingBrand(null)}>
                 {t('common.close')}
               </Button>
             </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
