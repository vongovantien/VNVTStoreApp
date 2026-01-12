import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Eye, Tag, Clock } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
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
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';
import { useToast } from '@/store';

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
  const pageSize: number = PageSize.DEFAULT;

  // Sorting
  const [sortField, setSortField] = useState<string>('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);

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
      header: t('admin.columns.code'),
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
      header: t('admin.columns.type'),
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
      header: t('admin.columns.discount'),
      accessor: (p) => (
        <div className="font-semibold text-rose-600">
          {p.discountType === 'PERCENTAGE' ? `${p.discountValue}%` : formatCurrency(p.discountValue)}
        </div>
      ),
      sortable: true
    },
    {
      id: 'date',
      header: t('admin.columns.date'),
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
      header: 'Usage Limit',
      accessor: (p) => p.usageLimit ?? '∞',
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('admin.columns.status'),
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
    {
      id: 'actions',
      header: t('admin.columns.action'),
      accessor: (p) => (
        <div className="flex items-center justify-center gap-2">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            onClick={() => setViewingPromotion(p)}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            onClick={() => openEdit(p)}
          >
            <Edit2 size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            onClick={() => confirmDelete(p)}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    }
  ];

  // Handler Wrappers
  const handleCreate = async (data: any) => {
    // Map data if needed
    await createMutation.mutateAsync(data);
  };

  const handleUpdate = async (data: any) => {
    if (!editingPromotion) return;
    await updateMutation.mutateAsync({
      id: editingPromotion.code, // Map code to id for generic hook
      data: data
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
    refetch();
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.promotions')}</h1>
      </div>

      <DataTable
        columns={columns}
        data={promotions}
        keyField="code"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}

        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setCurrentPage}

        externalSortField={sortField}
        externalSortDir={sortDir}
        onExternalSort={(f, d) => { setSortField(f); setSortDir(d as any); }}

        onAdd={openCreate}
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
          setCurrentPage(1);
        }}
        advancedFilterDefs={[
          { id: 'search', label: 'Search', type: 'text', placeholder: 'Search by Name/Code...' },
          { id: 'type', label: 'Type', type: 'select', options: [{ value: 'voucher', label: 'Voucher' }, { value: 'flash_sale', label: 'Flash Sale' }] },
          { id: 'isActive', label: 'Status', type: 'select', options: [{ value: 'true', label: 'Active' }, { value: 'false', label: 'Inactive' }] }
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
