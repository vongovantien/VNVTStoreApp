
import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Ruler } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { unitService, type CatalogUnitDto, type CreateUnitRequest, type UpdateUnitRequest } from '@/services';
import { DataTable, type DataTableColumn } from '@/components/common';
import { AdminPageHeader } from '@/components/admin';
import { useEntityManager, useUnits } from '@/hooks';
import { CatalogUnitForm, type CatalogUnitFormData } from './forms/CatalogUnitForm';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';
import { SearchCondition } from '@/services/baseService';

export default function UnitsPage() {
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
  } = useUnits(searchParams);

  const units = result?.data?.items || [];
  const totalItems = result?.data?.totalItems || 0;
  const totalPages = Math.ceil(totalItems / pageSize);

  // Entity Manager
  const {
      isFormOpen,
      editingItem: editingUnit,
      itemToDelete: unitToDelete,
      isDeleting,
      openCreate,
      openEdit,
      closeForm,
      confirmDelete,
      cancelDelete,
      createMutation,
      updateMutation,
      deleteMutation
  } = useEntityManager<CatalogUnitDto, CreateUnitRequest, UpdateUnitRequest>({
      service: unitService,
      queryKey: ['units']
  });

  const { mutate: createUnit, isPending: isCreating } = createMutation;
  const { mutate: updateUnit, isPending: isUpdating } = updateMutation;
  const { mutate: deleteUnit } = deleteMutation;

  // View State
  const [viewingUnit, setViewingUnit] = useState<CatalogUnitDto | null>(null);

  // Selection
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Bulk Delete State
  const [itemsToDelete, setItemsToDelete] = useState<CatalogUnitDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => unitService.deleteMultiple(codes),
    onSuccess: () => {
      setItemsToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['units'] });
    }
  });

  const handleBulkDelete = (items: CatalogUnitDto[]) => {
    setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
      bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

  const handleFormSubmit = async (formData: CatalogUnitFormData) => {
    const apiData = {
        name: formData.name,
        isActive: formData.isActive
    };
    
    if (editingUnit) {
      await updateUnit({
          id: editingUnit.code, 
          data: apiData as unknown as UpdateUnitRequest
      }, {
          onSuccess: () => closeForm()
      });
    } else {
      await createUnit(apiData as unknown as CreateUnitRequest, {
          onSuccess: () => closeForm()
      });
    }
  };

  const handleDelete = () => {
    if (unitToDelete) {
      deleteUnit(unitToDelete.code);
    }
  };

  const isSubmitting = isCreating || isUpdating;

  // Initial Data
  const prepareInitialData = (): Partial<CatalogUnitFormData> | undefined => {
      if (!editingUnit) return undefined;
      return {
          name: editingUnit.name,
          isActive: editingUnit.isActive
      };
  };

  // Columns
  const columns: DataTableColumn<CatalogUnitDto>[] = [
    {
      id: 'name',
      header: t('common.fields.unitName'),
      accessor: 'name',
      sortable: true,
      className: 'font-medium'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (unit) => (
        <Badge color={unit.isActive !== false ? 'success' : 'secondary'}>
          {unit.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
        </Badge>
      ),
      className: 'text-center w-[120px]',
    },
    {
        id: 'actions',
        header: '',
        className: 'w-[100px]',
        accessor: (unit) => (
          <TableActions
            onView={() => setViewingUnit(unit)}
            onEdit={() => openEdit(unit)}
            onDelete={() => confirmDelete(unit)}
          />
        )
      }
  ];

  // Stats
  const stats: StatItem[] = [
      {
          label: t('common.fields.unit'),
          value: totalItems,
          icon: <Ruler size={24} />,
          color: 'blue',
      }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.units" 
        subtitle="admin.subtitles.units" 
      />

      <StatsCards stats={stats} />

      <DataTable
        data={units}
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
             { id: 'Name', label: t('common.fields.unitName'), type: 'text', placeholder: t('common.placeholders.search') },
             { id: 'IsActive', label: t('common.fields.status'), type: 'select', options: [{ value: 'true', label: t('admin.status.active') }, { value: 'false', label: t('admin.status.inactive') }] }
        ]}
        onAdvancedSearch={setFilters}

         // Export
        onExportAllData={async () => {
           const res = await unitService.getAll(10000);
           return res.data?.items || [];
        }}
        exportFilename="units"

        keyField="code"
        enableSelection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onBulkDelete={handleBulkDelete}
        onAdd={() => openCreate()}
        onEdit={(item) => openEdit(item)}
        onDelete={(item) => confirmDelete(item)}
        onView={(item) => setViewingUnit(item)}
      />

       {/* Form Modal */}
       {isFormOpen && (
        <CatalogUnitForm
            initialData={prepareInitialData()}
            onSubmit={handleFormSubmit}
            onCancel={closeForm}
            isLoading={isSubmitting}
            modalOpen={isFormOpen}
            modalTitle={editingUnit ? t('admin.actions.edit') : t('admin.actions.create')}
        />
       )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!unitToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete', { name: unitToDelete?.name })}
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
      {viewingUnit && (
        <Modal
          isOpen={!!viewingUnit}
          onClose={() => setViewingUnit(null)}
          title={t('admin.actions.viewDetails')}
          size="md"
        >
          <div className="space-y-6">
             <div className="flex items-center gap-4 border-b border-gray-100 pb-4">
                <div className="w-16 h-16 bg-blue-50 rounded-lg flex items-center justify-center text-blue-500">
                    <Ruler size={32} />
                </div>
                <div>
                    <h2 className="text-xl font-bold">{viewingUnit.name}</h2>
                    <Badge color={viewingUnit.isActive !== false ? 'success' : 'secondary'} className="mt-1">
                        {viewingUnit.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
                    </Badge>
                </div>
             </div>
             
             {/* Removed Price/Rate details as this is global unit catalog */}
             <div className="text-sm text-gray-500">
                Units in the catalog define the standard measurement types (e.g. Box, Roll, Kg). 
                Specific conversion rates and prices are defined per product.
             </div>

              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.code')}</label>
                <p className="text-sm mt-1 font-mono text-tertiary">{viewingUnit.code}</p>
             </div>
             
             <div className="flex justify-end pt-4">
               <Button onClick={() => setViewingUnit(null)}>
                 {t('common.close')}
               </Button>
             </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
