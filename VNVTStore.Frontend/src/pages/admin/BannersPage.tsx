import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Eye, Edit2, Trash2, ExternalLink } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { useBanners, useCreateBanner, useUpdateBanner, useDeleteBanner } from '@/hooks/useBanners';
import { BannerForm, BannerFormData } from './forms/BannerForm';
import { useToast } from '@/store';
import { BannerDto } from '@/services/bannerService';
import { formatDate } from '@/utils/format';

const BannersPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;
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
      header: t('admin.columns.title') || 'Title',
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
      header: 'Link',
      accessor: (banner) => banner.linkUrl ? (
        <a
          href={banner.linkUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-blue-600 hover:underline flex items-center gap-1 text-sm"
        >
          {banner.linkText || 'Link'} <ExternalLink size={12} />
        </a>
      ) : <span className="text-slate-400">-</span>,
    },
    {
      id: 'priority',
      header: t('admin.columns.priority') || 'Priority',
      accessor: 'priority',
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('admin.columns.status'),
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
      header: t('admin.columns.createdAt') || 'Created At',
      accessor: (banner) => <span className="text-slate-500">{formatDate(banner.createdAt)}</span>,
      sortable: true
    },
    {
      id: 'actions',
      header: t('admin.columns.action'),
      accessor: (banner) => (
        <div className="flex items-center justify-center gap-2">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            title={t('admin.actions.view')}
            onClick={() => setViewingBanner(banner)}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            title={t('admin.actions.edit')}
            onClick={() => openEdit(banner)}
          >
            <Edit2 size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            title={t('admin.actions.delete')}
            onClick={() => setBannerToDelete(banner)}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    }
  ];

  const handleCreate = (data: BannerFormData) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        setIsFormOpen(false);
        toast.success(t('messages.createSuccess') || 'Banner created successfully');
      },
      onError: () => toast.error(t('messages.createError') || 'Failed to create banner'),
    });
  };

  const handleUpdate = (data: BannerFormData) => {
    if (!editingBanner) return;
    updateMutation.mutate({ code: editingBanner.code, data }, {
      onSuccess: () => {
        setIsFormOpen(false);
        setEditingBanner(null);
        toast.success(t('messages.updateSuccess') || 'Banner updated successfully');
      },
      onError: () => toast.error(t('messages.updateError') || 'Failed to update banner'),
    });
  };

  const handleDelete = () => {
    if (bannerToDelete) {
      deleteMutation.mutate(bannerToDelete.code, {
        onSuccess: () => {
          setBannerToDelete(null);
          toast.success(t('common.deleteSuccess'));
        },
        onError: () => toast.error(t('messages.deleteError') || 'Failed to delete banner'),
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
        .catch(() => toast.error('Failed to delete some banners'));
    }
  };

  const openEdit = (banner: BannerDto) => {
    setEditingBanner(banner);
    setIsFormOpen(true);
  };

  const handleReset = () => {
    setSearchQuery('');
    setCurrentPage(1);
    refetch();
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.banners') || 'Banners'}</h1>
      </div>

      <DataTable
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
            label: 'Title',
            type: 'text',
            placeholder: 'Search title...'
          },
          {
            id: 'status',
            label: 'Status',
            type: 'select',
            options: [
              { value: 'active', label: 'Active' },
              { value: 'inactive', label: 'Inactive' }
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
        onAdd={() => { setEditingBanner(null); setIsFormOpen(true); }}
        onView={(item) => setViewingBanner(item)}
        onEdit={openEdit}
        onDelete={(item) => setBannerToDelete(item)}
        onBulkDelete={() => setShowBulkConfirm(true)}

        // Selection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}

        enableColumnVisibility={true}
        exportFilename="banners_export"
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingBanner ? (t('admin.actions.edit') + ' Banner') : (t('admin.actions.create') + ' Banner')}
        size="lg"
      >
        <BannerForm
          initialData={editingBanner ? {
            title: editingBanner.title,
            content: editingBanner.content || '',
            linkUrl: editingBanner.linkUrl || '',
            linkText: editingBanner.linkText || '',
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
        title={t('admin.actions.view') + ' Banner'}
        size="md"
      >
        {viewingBanner && (
          <div className="space-y-4">
            <div>
              <h3 className="text-lg font-bold text-slate-800 dark:text-white">{viewingBanner.title}</h3>
              <Badge color={viewingBanner.isActive ? 'success' : 'secondary'} className="mt-1">
                {viewingBanner.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>

            {viewingBanner.content && (
              <div className="p-3 bg-slate-50 dark:bg-slate-900 rounded-lg">
                <p className="text-sm text-slate-600 dark:text-slate-300">{viewingBanner.content}</p>
              </div>
            )}

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-slate-500 block">Priority</span>
                <span className="font-medium">{viewingBanner.priority}</span>
              </div>
              <div>
                <span className="text-slate-500 block">Created At</span>
                <span className="font-medium">{formatDate(viewingBanner.createdAt)}</span>
              </div>
              {viewingBanner.linkUrl && (
                <div className="col-span-2">
                  <span className="text-slate-500 block">Link</span>
                  <a href={viewingBanner.linkUrl} target="_blank" className="text-blue-600 hover:underline break-all">
                    {viewingBanner.linkUrl}
                  </a>
                </div>
              )}
            </div>

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingBanner(null)}>
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
