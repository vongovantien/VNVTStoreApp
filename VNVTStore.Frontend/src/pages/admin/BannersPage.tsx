import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ExternalLink } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { useBanners, useCreateBanner, useUpdateBanner, useDeleteBanner } from '@/hooks/useBanners';
import { BannerForm, BannerFormData } from './forms/BannerForm';
import { useToast, useAuthStore } from '@/store';
import { bannerService, BannerDto } from '@/services/bannerService';
import { formatDate } from '@/utils/format';
import { AdminPageHeader } from '@/components/admin';
import { PaginationDefaults } from '@/constants';

const BannersPage = () => {
  const { t } = useTranslation();
  const toast = useToast();
  const { hasPermission } = useAuthStore();

  const canCreate = hasPermission('BANNERS_CREATE');
  const canUpdate = hasPermission('BANNERS_UPDATE');
  const canDelete = hasPermission('BANNERS_DELETE');

  // State
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const pageSize = PaginationDefaults.PAGE_SIZE;
  const [searchQuery, setSearchQuery] = useState('');

  // Fetch Data
  const { data: bannersResponse, isLoading, refetch } = useBanners({
    pageIndex: currentPage,
    pageSize,
    search: searchQuery || undefined,
  });

  const banners: BannerDto[] = bannersResponse?.data?.items || [];
  const totalItems = bannersResponse?.data?.totalItems || 0;
  const totalPages = Math.ceil(totalItems / pageSize);

  // Mutations
  const createMutation = useCreateBanner();
  const updateMutation = useUpdateBanner();
  const deleteMutation = useDeleteBanner();

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingBanner, setEditingBanner] = useState<BannerDto | null>(null);
  const [viewingBanner, setViewingBanner] = useState<BannerDto | null>(null);

  // Selection State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const selectedToDelete = banners.filter(item => selectedIds.has(item.code));
  const [showBulkConfirm, setShowBulkConfirm] = useState(false);
  const [bannerToDelete, setBannerToDelete] = useState<BannerDto | null>(null);

  const columns: DataTableColumn<BannerDto>[] = [
    {
      id: 'title',
      header: t('common.fields.title'),
      accessor: (banner) => (
        <div>
          <p className="font-medium text-slate-800 dark:text-slate-100">{banner.title}</p>
          <p className="text-xs text-slate-500 truncate max-w-[200px]">{banner.content}</p>
        </div>
      ),
      sortable: true
    },
    {
      id: 'link',
      header: t('common.fields.linkUrl'),
      accessor: (banner) => banner.linkUrl ? (
        <a
          href={banner.linkUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-blue-600 hover:underline flex items-center gap-1 text-sm"
        >
          {banner.linkText || t('common.fields.linkUrl')} <ExternalLink size={12} />
        </a>
      ) : <span className="text-slate-400">-</span>,
    },
    {
      id: 'priority',
      header: t('common.fields.priority'),
      accessor: 'priority',
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (banner) => (
        <Badge
          color={banner.isActive ? 'success' : 'secondary'}
          variant="outline"
        >
          {banner.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'createdAt',
      header: t('common.fields.date'),
      accessor: (banner) => <span className="text-slate-500">{formatDate(banner.createdAt)}</span>,
      sortable: true
    },

  ];

  const handleCreate = (data: BannerFormData) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        setIsFormOpen(false);
        toast.success(t('common.messages.createSuccess'));
      },
      onError: () => toast.error(t('common.messages.createError')),
    });
  };

  const handleUpdate = (data: BannerFormData) => {
    if (!editingBanner) return;
    updateMutation.mutate({ code: editingBanner.code, data }, {
      onSuccess: () => {
        setIsFormOpen(false);
        setEditingBanner(null);
        toast.success(t('common.messages.updateSuccess'));
      },
      onError: () => toast.error(t('common.messages.updateError')),
    });
  };

  const handleDelete = () => {
    if (bannerToDelete) {
      deleteMutation.mutate(bannerToDelete.code, {
        onSuccess: () => {
          setBannerToDelete(null);
          toast.success(t('common.deleteSuccess'));
        },
        onError: () => toast.error(t('common.messages.deleteError')),
      });
    } else if (selectedToDelete.length > 0) {
      // Bulk delete simulation - wait for backend bulk delete or loop
      // Assuming loop for now or bulk if supported.
      // Hook only provides single delete.
      Promise.all(selectedToDelete.map(b => deleteMutation.mutateAsync(b.code)))
        .then(() => {
          setSelectedIds(new Set());
          setShowBulkConfirm(false);
          toast.success(t('common.deleteSuccess'));
        })
        .catch(() => toast.error(t('common.messages.deleteSomeError')));
    }
  };

  const openEdit = (banner: BannerDto) => {
    setEditingBanner(banner);
    setIsFormOpen(true);
  };

  const handleReset = () => {
    setSearchQuery('');
    setCurrentPage(PaginationDefaults.PAGE_INDEX);

  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.banners"
        subtitle="admin.subtitles.banners"
      />

      <DataTable<BannerDto>
        columns={columns}
        data={banners}
        keyField="code"
        isLoading={isLoading}

        // Search
        onAdvancedSearch={(filters) => {
          // Mapping advanced filters to simple search for now if backend doesn't support complex
          if (filters.search) setSearchQuery(filters.search);
        }}
        onReset={handleReset}
        advancedFilterDefs={[
          {
            id: 'title',
            label: t('common.fields.title'),
            type: 'text',
            placeholder: t('common.placeholders.enterTitle')
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


        // Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setCurrentPage}

        // Actions
        {...(canCreate ? { onAdd: () => { setEditingBanner(null); setIsFormOpen(true); } } : {})}
        onRefresh={() => refetch()}
        onView={(item) => setViewingBanner(item)}
        {...(canUpdate ? { onEdit: openEdit } : {})}
        {...(canDelete ? { onDelete: (item) => setBannerToDelete(item) } : {})}
        {...(canDelete ? { onBulkDelete: () => setShowBulkConfirm(true) } : {})}

        // Selection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}

        enableColumnVisibility={true}
        exportFilename="banners_export"
        onExportAllData={async (): Promise<BannerDto[]> => {
          const response = await bannerService.getAll(10000);
          return (response.data?.items || []) as BannerDto[];
        }}
        
        // Import
        onImport={async (file) => {
          try {
              await bannerService.import(file);
              toast.success(t('messages.importSuccess'));
              refetch();
          } catch (err: any) {
              toast.error(err.message || t('messages.importError'));
          }
        }}
        importTemplateUrl="/api/v1/banners/template"
        importTitle={t('common.importData')}
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingBanner ? (t('admin.actions.edit') + ' ' + t('admin.banners')) : (t('admin.actions.create') + ' ' + t('admin.banners'))}
        size="lg"
      >
        <BannerForm
          initialData={editingBanner ? {
              title: editingBanner.title,
              content: editingBanner.content || '',
              linkUrl: editingBanner.linkUrl || '',
              linkText: editingBanner.linkText || '',
              imageURL: editingBanner.imageURL || '',
              priority: editingBanner.priority,
              isActive: editingBanner.isActive
          } : undefined}
          onSubmit={editingBanner ? handleUpdate : handleCreate}
          onCancel={() => setIsFormOpen(false)}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      </Modal>

      {/* View Modal */}
      <Modal
        isOpen={!!viewingBanner}
        onClose={() => setViewingBanner(null)}
        title={t('admin.actions.view') + ' ' + t('admin.banners')}
        size="md"
      >
        {viewingBanner && (
          <div className="space-y-4">
            <div>
              <h3 className="text-lg font-bold text-slate-800 dark:text-white">{viewingBanner.title}</h3>
              <Badge color={viewingBanner.isActive ? 'success' : 'secondary'} className="mt-1">
                {viewingBanner.isActive ? t('common.status.active') : t('common.status.inactive')}
              </Badge>
            </div>

            {viewingBanner.content && (
              <div className="p-3 bg-slate-50 dark:bg-slate-900 rounded-lg">
                <p className="text-sm text-slate-600 dark:text-slate-300">{viewingBanner.content}</p>
              </div>
            )}

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-slate-500 block">{t('common.fields.priority')}</span>
                <span className="font-medium">{viewingBanner.priority}</span>
              </div>
              <div>
                <span className="text-slate-500 block">{t('common.fields.createdAt')}</span>
                <span className="font-medium">{formatDate(viewingBanner.createdAt)}</span>
              </div>
              {viewingBanner.linkUrl && (
                <div className="col-span-2">
                  <span className="text-slate-500 block">{t('common.fields.linkUrl')}</span>
                  <a href={viewingBanner.linkUrl} target="_blank" className="text-blue-600 hover:underline break-all" rel="noreferrer">
                    {viewingBanner.linkUrl}
                  </a>
                </div>
              )}
            </div>

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingBanner(null)} variant="ghost">
                {t('common.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!bannerToDelete || showBulkConfirm}
        onClose={() => { setBannerToDelete(null); setShowBulkConfirm(false); setSelectedIds(new Set()); }}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: bannerToDelete ? 1 : selectedToDelete.length })}
        isLoading={deleteMutation.isPending}
        variant="danger"
      />
    </div>
  );
};

export default BannersPage;
