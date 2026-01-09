
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Loader2, Search } from 'lucide-react';
import { Button, Badge, Modal, Pagination, ConfirmDialog, Input } from '@/components/ui';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { ColumnVisibility } from '@/components/admin/ColumnVisibility';
import { useBanners, useCreateBanner, useUpdateBanner, useDeleteBanner } from '@/hooks/useBanners';
import { BannerForm, BannerFormData } from './forms/BannerForm';
import { useToast } from '@/store';
import { BannerDto } from '@/services/bannerService';

const BannersPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearchActive, setIsSearchActive] = useState(false); // Toggle search bar
  const pageSize = 10;
  
  // Fetch Data
  const { data: bannersResponse, isLoading } = useBanners({
    pageIndex: currentPage,
    pageSize,
    search: searchQuery || undefined,
  });

  // Access nested data: ApiResponse -> PagedResult -> items
  const banners = bannersResponse?.data?.items || [];
  const totalItems = bannersResponse?.data?.totalItems || 0;
  const totalPages = Math.ceil(totalItems / pageSize);

  // Mutations
  const createMutation = useCreateBanner();
  const updateMutation = useUpdateBanner();
  const deleteMutation = useDeleteBanner();

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingBanner, setEditingBanner] = useState<BannerDto | null>(null);
  const [bannerToDelete, setBannerToDelete] = useState<BannerDto | null>(null);

  // Column Visibility
  const [visibleColumns, setVisibleColumns] = useState<string[]>([
    'title', 'content', 'link', 'priority', 'status', 'actions'
  ]);

  const columnsDef = [
    { id: 'title', label: 'Title' },
    { id: 'content', label: 'Content' },
    { id: 'link', label: 'Link' },
    { id: 'priority', label: 'Priority' },
    { id: 'status', label: 'Status' },
    { id: 'actions', label: 'Actions' },
  ];

  const handleCreate = (data: BannerFormData) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        setIsFormOpen(false);
        toast.success('Banner created successfully');
      },
      onError: () => toast.error('Failed to create banner'),
    });
  };

  const handleUpdate = (data: BannerFormData) => {
    if (!editingBanner) return;
    updateMutation.mutate({ code: editingBanner.code, data }, {
      onSuccess: () => {
        setIsFormOpen(false);
        setEditingBanner(null);
        toast.success('Banner updated successfully');
      },
      onError: () => toast.error('Failed to update banner'),
    });
  };

  const handleDelete = () => {
    if (!bannerToDelete) return;
    deleteMutation.mutate(bannerToDelete.code, {
      onSuccess: () => {
        setBannerToDelete(null);
        toast.success('Banner deleted successfully');
      },
      onError: () => toast.error('Failed to delete banner'),
    });
  };

  const openEdit = (banner: BannerDto) => {
    setEditingBanner(banner);
    setIsFormOpen(true);
  };

  if (isLoading) {
    return (
        <div className="flex justify-center items-center h-96">
            <Loader2 className="w-8 h-8 animate-spin text-primary" />
        </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Banners</h1>
          <p className="text-muted-foreground">Manage website banners and announcements</p>
        </div>
        <div className="flex gap-2">
             <ColumnVisibility
                columns={columnsDef}
                visibleColumns={visibleColumns}
                onChange={setVisibleColumns}
              />
        </div>
      </div>

      <div className="flex flex-col gap-4">
        <AdminToolbar
            selectedCount={0}
            onAdd={() => { setEditingBanner(null); setIsFormOpen(true); }}
            onSearchClick={() => setIsSearchActive(!isSearchActive)}
            isSearchActive={isSearchActive}
            onReset={() => { setSearchQuery(''); setCurrentPage(1); }}
        />
        
        {isSearchActive && (
            <div className="flex items-center gap-2 max-w-sm transition-all animate-in fade-in slide-in-from-top-2">
                <Search className="w-4 h-4 text-muted-foreground" />
                <Input
                    placeholder="Search banners by title..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="flex-1"
                />
            </div>
        )}
      </div>

      <div className="rounded-md border bg-card">
        <div className="relative w-full overflow-auto">
          <table className="w-full caption-bottom text-sm">
            <thead className="[&_tr]:border-b">
              <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                {visibleColumns.includes('title') && <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Title</th>}
                {visibleColumns.includes('content') && <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Content</th>}
                {visibleColumns.includes('link') && <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Link</th>}
                {visibleColumns.includes('priority') && <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Priority</th>}
                {visibleColumns.includes('status') && <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Status</th>}
                {visibleColumns.includes('actions') && <th className="h-12 px-4 text-right align-middle font-medium text-muted-foreground">Actions</th>}
              </tr>
            </thead>
            <tbody className="[&_tr:last-child]:border-0">
              {banners.length === 0 ? (
                <tr>
                  <td colSpan={visibleColumns.length} className="p-4 text-center text-muted-foreground">
                    No banners found.
                  </td>
                </tr>
              ) : (
                banners.map((banner) => (
                  <tr key={banner.code} className="border-b transition-colors hover:bg-muted/50">
                    {visibleColumns.includes('title') && <td className="p-4 align-middle font-medium">{banner.title}</td>}
                    {visibleColumns.includes('content') && <td className="p-4 align-middle truncate max-w-[200px]">{banner.content}</td>}
                    {visibleColumns.includes('link') && (
                        <td className="p-4 align-middle text-blue-600 truncate max-w-[150px]">
                            {banner.linkUrl}
                        </td>
                    )}
                    {visibleColumns.includes('priority') && <td className="p-4 align-middle">{banner.priority}</td>}
                    {visibleColumns.includes('status') && (
                        <td className="p-4 align-middle">
                            <Badge 
                                color={banner.isActive ? 'success' : 'secondary'}
                                variant="outline"
                            >
                                {banner.isActive ? 'Active' : 'Inactive'}
                            </Badge>
                        </td>
                    )}
                    {visibleColumns.includes('actions') && (
                        <td className="p-4 align-middle text-right">
                            <div className="flex justify-end gap-2">
                                <Button variant="ghost" size="sm" onClick={() => openEdit(banner)}>Edit</Button>
                                <Button variant="ghost" size="sm" className="text-red-600" onClick={() => setBannerToDelete(banner)}>Delete</Button>
                            </div>
                        </td>
                    )}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {totalPages > 1 && (
        <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
            totalItems={totalItems}
            pageSize={pageSize}
        />
      )}

      <Modal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingBanner ? 'Edit Banner' : 'Create Banner'}
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

      <ConfirmDialog
        isOpen={!!bannerToDelete}
        onClose={() => setBannerToDelete(null)}
        onConfirm={handleDelete}
        title="Delete Banner"
        message={`Are you sure you want to delete banner "${bannerToDelete?.title}"?`}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default BannersPage;
