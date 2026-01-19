import { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Eye, Tag, Clock } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog, TableActions } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';
import { PromotionForm } from './forms/PromotionForm';
import {
  useEntityManager,
} from '@/hooks'; // We will assume usePromotions hook logic is handled via useEntityManager or we need a hook? 
// useEntityManager requires a service. We have promotionService.
// We also need a fetch hook for data. `usePromotions` doesn't specific exist as a hook file yet? 
// Wait, I didn't create `usePromotions.ts`. I should use `useQuery` or `useEntityManager`'s integrated hook if it has one?
// `useEntityManager` typically handles mutations. Data fetching is usually `useQuery`.
// I will create a simple `usePromotions` hook in this file or use `useQuery` directly for "DRY" if I don't want a separate file.
// But following pattern, `useProducts` exists. I should probably have `usePromotions`.
// However, to keep it simple and within this file ("OOP encapsulate?"), I can use `useQuery`.
// Let's use `useQuery` from `@tanstack/react-query` directly here or import a standard hook.
// I'll assume I need to fetch data.

import { promotionService, Promotion, CreatePromotionRequest, UpdatePromotionRequest } from '@/services/promotionService';
import { useQuery } from '@tanstack/react-query';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { ImportModal } from '@/components/common/ImportModal';
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';
import { useToast } from '@/store';
import { AdminPageHeader } from '@/components/admin';

// We need a hook to fetch promotions compatible with DataTable
const usePromotions = (params: any) => {
  return useQuery({
    queryKey: ['promotions', params],
    queryFn: () => promotionService.getAll(params),
    placeholderData: (previousData) => previousData,
  });
};

export const PromotionsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});
  const [pageSize, setPageSize] = useState<number>(PaginationDefaults.PAGE_SIZE);

  // Sorting
  const [sortField, setSortField] = useState<string>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);

  // Import
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);
  const handleImportPromotion = async (file: File) => {
    try {
        await promotionService.import(file);
        toast.success(t('common.importSuccess') || 'Import successful');
        refetch();
    } catch (error) {
        toast.error(t('common.importError') || 'Import failed');
        throw error;
    }
  };

  // Fetch API
  const {
    data: promotionsData,
    isLoading,
    isFetching,
    isError,
    error,
    refetch,
  } = usePromotions({
    pageIndex: currentPage,
    pageSize,
    search: searchQuery || undefined,
    sortField,
    sortDir,
    ...advancedFilters
  });

  const promotions: Promotion[] = promotionsData?.data?.items || [];
  const totalItems: number = promotionsData?.data?.totalItems || 0;
  const totalPages: number = Math.ceil(totalItems / pageSize);

  // Mutations and State via useEntityManager
  const {
    isFormOpen,
    editingItem: editingPromotion,
    itemToDelete: promotionToDelete,
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
  } = useEntityManager<Promotion, CreatePromotionRequest, UpdatePromotionRequest>({
    service: promotionService,
    queryKey: ['promotions'],
  });

  // Selection
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const selectedToDelete = promotions.filter(p => selectedIds.has(p.code));
  const [showBulkConfirm, setShowBulkConfirm] = useState(false);

  // View State
  const [viewingPromotion, setViewingPromotion] = useState<Promotion | null>(null);

  // Column Definitions
  const columns: DataTableColumn<Promotion>[] = [
    {
      id: 'code',
      header: t('common.fields.code'),
      accessor: (p) => (
        <div className="flex flex-col">
          <span className="font-bold text-primary">{p.code}</span>
          <span className="text-xs text-slate-500">{p.name}</span>
        </div>
      ),
      sortable: true
    },
    {
      id: 'type',
      header: t('common.fields.type'),
      accessor: (p) => (
        <Badge variant="outline" className="flex items-center gap-1 w-fit">
          {p.productCodes && p.productCodes.length > 0 ? (
            <><Clock size={12} /> Flash Sale</>
          ) : (
            <><Tag size={12} /> Voucher</>
          )}
        </Badge>
      )
    },
    {
      id: 'discount',
      header: t('common.fields.discount'),
      accessor: (p) => (
        <div className="font-semibold text-rose-600">
          {p.discountType === 'PERCENTAGE' ? `${p.discountValue}%` : formatCurrency(p.discountValue)}
        </div>
      ),
      sortable: true
    },
    {
      id: 'date',
      header: t('common.fields.date'),
      accessor: (p) => (
        <div className="text-xs flex flex-col">
          <span>{formatDate(p.startDate)}</span>
          <span className="text-slate-400">to</span>
          <span>{formatDate(p.endDate)}</span>
        </div>
      )
    },
    {
      id: 'usage',
      header: t('common.fields.usageLimit'),
      accessor: (p) => p.usageLimit ?? '∞',
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (p) => (
        <Badge
          color={p.isActive ? 'success' : 'error'}
          size="sm"
          variant="outline"
        >
          {p.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },

  ];

  // Handler Wrappers
  const handleCreate = async (data: any) => {
    // Sanitizing nulls to undefined to match CreatePromotionRequest
    const cleanData: CreatePromotionRequest = {
      ...data,
      minOrderAmount: data.minOrderAmount ?? undefined,
      maxDiscountAmount: data.maxDiscountAmount ?? undefined,
      usageLimit: data.usageLimit ?? undefined,
      productCodes: data.productCodes ?? undefined,
    };
    await createMutation.mutateAsync(cleanData);
  };

  const handleUpdate = async (data: any) => {
    if (!editingPromotion) return;
    const cleanData: UpdatePromotionRequest = {
      ...data,
      minOrderAmount: data.minOrderAmount ?? undefined,
      maxDiscountAmount: data.maxDiscountAmount ?? undefined,
      usageLimit: data.usageLimit ?? undefined,
      productCodes: data.productCodes ?? undefined,
    };
    await updateMutation.mutateAsync({
      id: editingPromotion.code,
      data: cleanData
    });
  };

  const handleDelete = async () => {
    if (promotionToDelete) {
      deleteMutation.mutate(promotionToDelete.code);
    } else if (selectedToDelete.length > 0) {
      try {
        await Promise.all(selectedToDelete.map(item => deleteMutation.mutateAsync(item.code)));
        setSelectedIds(new Set());
        setShowBulkConfirm(false);
        toast.success(t('common.deleteSuccess'));
      } catch (err) { }
    }
  };

  const handleReset = () => {
    setAdvancedFilters({});
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
    setSearchQuery('');
    setSortField('createdAt');
    setSortDir(SortDirection.DESC);

  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.promotions"
        subtitle="admin.subtitles.promotions"
      />

      <DataTable
        columns={columns}
        data={promotions}
        keyField="code"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}
        onAdd={() => openCreate()}
        onRefresh={() => refetch()}
        onImport={handleImportPromotion}
        importTemplateUrl="/templates/promotions_template.xlsx" 


        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setCurrentPage(PaginationDefaults.PAGE_INDEX);
        }}

        externalSortField={sortField}
        externalSortDir={sortDir}
        onExternalSort={(f, d) => { setSortField(f); setSortDir(d as any); }}
        
        onEdit={openEdit}
        onDelete={confirmDelete}
        onView={setViewingPromotion}

        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onBulkDelete={() => setShowBulkConfirm(true)}

        // Search
        onReset={handleReset}
        onAdvancedSearch={(filters) => {
          setAdvancedFilters(filters);
          setCurrentPage(PaginationDefaults.PAGE_INDEX);
        }}
        advancedFilterDefs={[
          { id: 'search', label: t('common.search'), type: 'text', placeholder: t('common.placeholders.search') },
          { id: 'type', label: t('common.fields.type'), type: 'select', options: [{ value: 'voucher', label: t('admin.types.voucher') }, { value: 'flash_sale', label: t('admin.types.flashSale') }] },
          { id: 'isActive', label: t('common.fields.status'), type: 'select', options: [{ value: 'true', label: t('common.status.active') }, { value: 'false', label: t('common.status.inactive') }] }
        ]}
      />

      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingPromotion ? t('admin.actions.edit') : t('admin.actions.create')}
        size="2xl"
      >
        <PromotionForm
          initialData={editingPromotion ? {
            ...editingPromotion,
            startDate: new Date(editingPromotion.startDate),
            endDate: new Date(editingPromotion.endDate),
          } : undefined}
          onSubmit={editingPromotion ? handleUpdate : handleCreate}
          onCancel={closeForm}
          isLoading={isSubmitting}
        />
      </Modal>

      <ConfirmDialog
        isOpen={!!promotionToDelete || showBulkConfirm}
        onClose={() => { cancelDelete(); setShowBulkConfirm(false); }}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: showBulkConfirm ? selectedToDelete.length : 1 })}
        confirmText={t('common.delete')}
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};

export default PromotionsPage;
