
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Building2, RefreshCw, Mail, Truck, Phone } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { supplierService, type SupplierDto, type CreateSupplierRequest, type UpdateSupplierRequest } from '@/services';
import { DataTable, type DataTableColumn, CommonColumns } from '@/components/common';
import { AdminPageHeader } from '@/components/admin';
import { useEntityManager, useSuppliersPaged } from '@/hooks';
import { SearchCondition } from '@/services/baseService';
import { useMemo } from 'react';
import { SupplierForm, type SupplierFormData } from './forms';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';
import { useQuery } from '@tanstack/react-query';

export default function SuppliersPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  // Data Fetching
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [filters, setFilters] = useState<Record<string, string>>({});

  const searchParams = useMemo(() => {
     const filterParams = [];
     if (filters.Name) filterParams.push({ field: 'Name', value: filters.Name, operator: SearchCondition.Contains });
     if (filters.TaxCode) filterParams.push({ field: 'TaxCode', value: filters.TaxCode, operator: SearchCondition.Contains });
     if (filters.Phone) filterParams.push({ field: 'Phone', value: filters.Phone, operator: SearchCondition.Contains });
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
  } = useSuppliersPaged(searchParams);

  const suppliers = result?.items || [];
  const totalItems = result?.totalItems || 0;
  const totalPages = Math.ceil(totalItems / pageSize);

  // Entity Manager (for CRUD state)
  const {
      isFormOpen,
      editingItem: editingSupplier,
      itemToDelete: supplierToDelete,
      isDeleting,
      openCreate,
      openEdit,
      closeForm,
      confirmDelete,
      cancelDelete,
      createMutation,
      updateMutation,
      deleteMutation
  } = useEntityManager<SupplierDto, CreateSupplierRequest, UpdateSupplierRequest>({
      service: supplierService,
      queryKey: ['suppliers']
  });

  const { mutate: createSupplier, isPending: isCreating } = createMutation;
  const { mutate: updateSupplier, isPending: isUpdating } = updateMutation;
  const { mutate: deleteSupplier } = deleteMutation;

  // View State
  const [viewingSupplier, setViewingSupplier] = useState<SupplierDto | null>(null);

  // Selection
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Bulk Delete State
  const [itemsToDelete, setItemsToDelete] = useState<SupplierDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => supplierService.deleteMultiple(codes),
    onSuccess: () => {
      setItemsToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
    }
  });

  const handleBulkDelete = (items: SupplierDto[]) => {
    setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
      bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

  const handleFormSubmit = async (formData: SupplierFormData) => {
    if (editingSupplier) {
      await updateSupplier({
          id: editingSupplier.code, 
          data: formData as UpdateSupplierRequest
      }, {
          onSuccess: () => closeForm()
      });
    } else {
      await createSupplier(formData as unknown as CreateSupplierRequest, {
          onSuccess: () => closeForm()
      });
    }
  };



  const handleImport = async (file: File) => {
      try {
          await supplierService.import(file);
          // Assuming toast is available globally or needs import. 
          // SuppliersPage uses useTranslation but not useToast explicitly in original snippet but let's check imports.
          // Original imports didn't show useToast. I will assume it's okay or add if needed.
          // Wait, ProductsPage used useToast. SuppliersPage might not have it setup.
          // Actually, handleApiResponse in baseService handles toast if we use it, but here we call service directly.
          // Let's just refetch.
          refetch();
      } catch (error) {
          console.error("Import error", error);
      }
  };

  const handleDelete = () => {
    if (supplierToDelete) {
      deleteSupplier(supplierToDelete.code);
    }
  };

  const isSubmitting = isCreating || isUpdating;

  const prepareInitialData = (): Partial<SupplierFormData> | undefined => {
      if (!editingSupplier) return undefined;
      return {
          name: editingSupplier.name,
          contactPerson: editingSupplier.contactPerson || undefined,
          email: editingSupplier.email || undefined,
          phone: editingSupplier.phone || undefined,
          address: editingSupplier.address || undefined,
          taxCode: editingSupplier.taxCode || undefined,
          bankAccount: editingSupplier.bankAccount || undefined,
          bankName: editingSupplier.bankName || undefined,
          notes: editingSupplier.notes || undefined,
          isActive: editingSupplier.isActive
      };
  };

  // Column definitions
  const columns: DataTableColumn<SupplierDto>[] = [
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: (supplier) => (
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 dark:text-indigo-400">
            <Building2 size={16} />
          </div>
          <div>
            <p className="font-medium text-slate-800 dark:text-slate-100">{supplier.name}</p>
            {supplier.taxCode && <p className="text-xs text-secondary">{t('common.fields.taxCode')}: {supplier.taxCode}</p>}
          </div>
        </div>
      ),
      sortable: true,
    },
    {
      id: 'contact',
      header: t('common.fields.contact'),
      accessor: (supplier) => (
        <div className="space-y-1">
          {supplier.contactPerson && (
            <p className="text-sm font-medium">{supplier.contactPerson}</p>
          )}
          {supplier.phone && (
            <div className="flex items-center gap-1.5 text-xs text-secondary">
              <Phone size={12} />
              <span>{supplier.phone}</span>
            </div>
          )}
          {supplier.email && (
            <div className="flex items-center gap-1.5 text-xs text-secondary">
              <Mail size={12} />
              <span className="truncate max-w-[150px]">{supplier.email}</span>
            </div>
          )}
        </div>
      ),
    },
    {
      id: 'address',
      header: t('common.fields.address'),
      accessor: 'address',
      className: 'hidden md:table-cell max-w-[200px] truncate'
    },
    CommonColumns.createStatusColumn(t),
    {
        id: 'actions',
        header: '',
        className: 'w-[100px]',
        accessor: (supplier) => (
          <TableActions
            onView={() => setViewingSupplier(supplier)}
            onEdit={() => openEdit(supplier)}
            onDelete={() => confirmDelete(supplier)}
          />
        )
      }
  ];

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['supplier-stats'],
      queryFn: () => supplierService.getStats(),
      staleTime: 60000,
  });

  const stats: StatItem[] = [
      {
          label: t('admin.stats.totalSuppliers'),
          value: statsData?.total || 0,
          icon: <Truck size={24} />,
          color: 'blue',
          loading: isStatsLoading
      },
      {
          label: t('admin.stats.active'),
          value: statsData?.active || 0,
          icon: <RefreshCw size={24} />,
          color: 'emerald',
          loading: isStatsLoading
      }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.suppliers"
        subtitle="admin.subtitles.suppliers"
      />

      <StatsCards stats={stats} />

      <DataTable
        data={suppliers as SupplierDto[]}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder={t('common.placeholders.search')}
        searchOptions={[
          { label: t('common.fields.name'), value: 'name' },
          { label: t('common.fields.phone'), value: 'phone' },
          { label: t('common.fields.email'), value: 'email' }
        ]}
        keyField="code"
        enableSelection
        // Selection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}

        // Pagination
        currentPage={page}
        pageSize={pageSize}
        totalItems={totalItems}
        totalPages={totalPages}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        
        onBulkDelete={handleBulkDelete}
        onAdd={() => openCreate()}
        onRefresh={() => refetch()}

        // Advanced Filter
        advancedFilterDefs={[
             { id: 'Name', label: t('common.fields.name'), type: 'text', placeholder: t('common.placeholders.search') },
             { id: 'TaxCode', label: t('common.fields.taxCode'), type: 'text', placeholder: 'MST...' },
             { id: 'Phone', label: t('common.fields.phone'), type: 'text', placeholder: 'SDT...' },
             { id: 'IsActive', label: t('common.fields.status'), type: 'select', options: [{ value: 'true', label: t('admin.status.active') }, { value: 'false', label: t('admin.status.inactive') }] }
        ]}
        onAdvancedSearch={setFilters}

        // Export
         onExportAllData={async () => {
           return supplierService.exportData(); // Use new generic helper
        }}
        onImport={handleImport}
        importTemplateUrl="/suppliers/template"
        importTitle={t('common.importData')}

        onEdit={(item) => openEdit(item)}
        onDelete={(item) => confirmDelete(item)}
        onView={(item) => setViewingSupplier(item)}
      />

       {/* Form Modal */}
       {isFormOpen && (
        <SupplierForm
            initialData={prepareInitialData()}
            onSubmit={handleFormSubmit}
            onCancel={closeForm}
            isLoading={isSubmitting}
            modalOpen={isFormOpen}
            modalTitle={editingSupplier ? t('admin.actions.edit') : t('admin.actions.create')}
        />
       )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!supplierToDelete}
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete', { name: supplierToDelete?.name })}
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
      {viewingSupplier && (
        <Modal
          isOpen={!!viewingSupplier}
          onClose={() => setViewingSupplier(null)}
          title={t('admin.actions.viewDetails')}
          size="lg"
        >
          <div className="space-y-6">
            <div className="flex items-center gap-4 border-b border-gray-100 dark:border-gray-800 pb-4">
              <div className="w-16 h-16 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 dark:text-indigo-400">
                <Building2 size={32} />
              </div>
              <div>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{viewingSupplier.name}</h2>
                <div className="mt-1">
                    <Badge color={viewingSupplier.isActive !== false ? 'success' : 'secondary'}>
                        {viewingSupplier.isActive !== false ? t('admin.status.active') : t('admin.status.inactive')}
                    </Badge>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-x-8 gap-y-6">
              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.taxCode')}</label>
                <p className="font-medium mt-1">{viewingSupplier.taxCode || t('common.none')}</p>
              </div>
              
              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.contactPerson')}</label>
                <p className="font-medium mt-1">{viewingSupplier.contactPerson || t('common.none')}</p>
              </div>

              <div className="col-span-2">
                 <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.address')}</label>
                 <p className="font-medium mt-1">{viewingSupplier.address || t('common.none')}</p>
              </div>

              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.phone')}</label>
                <div className="flex items-center gap-2 mt-1">
                    <Phone size={14} className="text-gray-400" />
                    <span className="font-medium">{viewingSupplier.phone || t('common.none')}</span>
                </div>
              </div>
              
              <div>
                 <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.email')}</label>
                 <div className="flex items-center gap-2 mt-1">
                    <Mail size={14} className="text-gray-400" />
                    <span className="font-medium">{viewingSupplier.email || t('common.none')}</span>
                </div>
              </div>

              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.bankName')}</label>
                <p className="font-medium mt-1">{viewingSupplier.bankName || t('common.none')}</p>
              </div>

              <div>
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.bankAccount')}</label>
                <p className="font-medium mt-1">{viewingSupplier.bankAccount || t('common.none')}</p>
              </div>

              <div className="col-span-2">
                <label className="text-xs text-secondary uppercase font-semibold">{t('common.fields.note')}</label>
                <p className="text-gray-600 dark:text-gray-300 mt-1 whitespace-pre-wrap text-sm">
                  {viewingSupplier.notes || t('common.none')}
                </p>
              </div>
            </div>
            
            <div className="flex justify-end pt-4 border-t border-gray-100 dark:border-gray-800">
              <Button onClick={() => setViewingSupplier(null)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
