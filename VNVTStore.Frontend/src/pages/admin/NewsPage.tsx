import { useState, lazy, Suspense } from 'react';
import { useTranslation } from 'react-i18next';
import { Trash2, Edit, Image as ImageIcon } from 'lucide-react';
const RichTextEditor = lazy(() => import('@/components/common/RichTextEditor'));
import { Button, Modal, ConfirmDialog } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { DataTable, CommonColumns } from '@/components/common';
import { formatDate } from '@/utils/format';
import { newsService, type NewsDto, type CreateNewsRequest, type UpdateNewsRequest } from '@/services/newsService';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { PaginationDefaults } from '@/constants';
import { useToast } from '@/store';

export const NewsPage = () => {
  const { t } = useTranslation();
  const { success, error: toastError } = useToast();
  const queryClient = useQueryClient();

  // State
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingNews, setEditingNews] = useState<NewsDto | null>(null);
  const [newsToDelete, setNewsToDelete] = useState<NewsDto | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [activeTab, setActiveTab ] = useState<'content' | 'seo'>('content');

  // Form State
  const [formData, setFormData] = useState({
    title: '',
    summary: '',
    content: '',
    thumbnail: '',
    author: '',
    isActive: true,
    metaTitle: '',
    metaDescription: '',
    metaKeywords: '',
    slug: ''
  });

  // Queries
  const { data: newsData, isLoading, isFetching } = useQuery({
    queryKey: ['admin-news', currentPage, pageSize, searchQuery],
    queryFn: () => newsService.search({
      pageIndex: currentPage,
      pageSize: pageSize,
      search: searchQuery
    }),
  });

  const newsItems = newsData?.data?.items || [];
  const totalPages = newsData?.data?.totalPages || 1;

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: CreateNewsRequest) => newsService.create(data),
    onSuccess: () => {
      success(t('messages.createSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-news'] });
      setIsModalOpen(false);
      resetForm();
    },
    onError: (err: Error) => toastError(err.message || t('messages.createError'))
  });

  const updateMutation = useMutation({
    mutationFn: ({ code, data }: { code: string, data: UpdateNewsRequest }) => newsService.update(code, data),
    onSuccess: () => {
      success(t('messages.updateSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-news'] });
      setIsModalOpen(false);
      resetForm();
    },
    onError: (err: Error) => toastError(err.message || t('messages.updateError'))
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => newsService.delete(code),
    onSuccess: () => {
      success(t('messages.deleteSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-news'] });
      setNewsToDelete(null);
    },
    onError: () => toastError(t('messages.deleteError'))
  });

  // Bulk Delete State
  const [itemsToDelete, setItemsToDelete] = useState<NewsDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => newsService.deleteMultiple(codes),
    onSuccess: () => {
      success(t('messages.deleteSuccess') || t('common.deleteSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-news'] });
      setSelectedIds(new Set());
      setItemsToDelete(null);
    },
    onError: () => toastError(t('messages.deleteError') || t('common.deleteError'))
  });

  const handleBulkDelete = (items: NewsDto[]) => {
      setItemsToDelete(items);
  };

  // Handlers
  const resetForm = () => {
    setFormData({
      title: '',
      summary: '',
      content: '',
      thumbnail: '',
      author: '',
      isActive: true,
      metaTitle: '',
      metaDescription: '',
      metaKeywords: '',
      slug: ''
    });
    setEditingNews(null);
    setActiveTab('content');
  };

  const handleEdit = (item: NewsDto) => {
    setEditingNews(item);
    setFormData({
      title: item.title,
      summary: item.summary || '',
      content: item.content || '',
      thumbnail: item.thumbnail || '',
      author: item.author || '',
      isActive: item.isActive,
      metaTitle: item.metaTitle || '',
      metaDescription: item.metaDescription || '',
      metaKeywords: item.metaKeywords || '',
      slug: item.slug || ''
    });
    setIsModalOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingNews) {
      updateMutation.mutate({ code: editingNews.code, data: formData as UpdateNewsRequest });
    } else {
      createMutation.mutate(formData as CreateNewsRequest);
    }
  };

  // Columns
  const columns = [
    {
      id: 'thumbnail',
      header: '',
      accessor: (row: NewsDto) => (
        <div className="w-12 h-12 rounded bg-tertiary flex items-center justify-center overflow-hidden">
          {row.thumbnail ? (
            <img src={row.thumbnail} alt={row.title} className="w-full h-full object-cover" />
          ) : (
            <ImageIcon size={20} className="text-tertiary" />
          )}
        </div>
      ),
      className: 'w-16'
    },
    {
      id: 'title',
      header: t('common.fields.title'),
      accessor: (row: NewsDto) => (
        <div className="max-w-[300px]">
          <p className="font-medium truncate">{row.title}</p>
          <p className="text-xs text-tertiary truncate">{row.summary}</p>
        </div>
      )
    },
    {
       id: 'author',
       header: t('common.fields.author'),
       accessor: (row: NewsDto) => row.author || t('common.none')
    },
    CommonColumns.createStatusColumn(t),
    {
      id: 'date',
      header: t('common.fields.date'),
      accessor: (row: NewsDto) => <span className="text-secondary text-sm">{formatDate(row.publishedAt || row.createdAt)}</span>
    }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.news"
        subtitle="admin.subtitles.news"
      />

      <DataTable
        columns={columns}
        data={newsItems}
        isLoading={isLoading || isFetching}
        keyField="code"
        
        currentPage={currentPage}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={setPageSize}

        onAdd={() => { resetForm(); setIsModalOpen(true); }}
        onRefresh={() => queryClient.invalidateQueries({ queryKey: ['admin-news'] })}
        onSearch={setSearchQuery}

        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onBulkDelete={handleBulkDelete}

        renderRowActions={(row) => (
          <div className="flex items-center gap-1">
            <button
              className="p-1.5 hover:bg-primary/10 rounded text-primary"
              title={t('common.actions.edit')}
              onClick={() => handleEdit(row)}
            >
              <Edit size={18} />
            </button>
            <button
              className="p-1.5 hover:bg-error/10 rounded text-error"
              title={t('common.actions.delete')}
              onClick={() => setNewsToDelete(row)}
            >
              <Trash2 size={18} />
            </button>
          </div>
        )}
      />

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingNews ? t('admin.news.edit') : t('admin.news.add')}
        size="xl"
        footer={
          <div className="flex justify-end gap-2">
             <Button variant="ghost" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
             <Button onClick={handleSubmit} isLoading={createMutation.isPending || updateMutation.isPending}>{t('common.save')}</Button>
          </div>
        }
      >
        <div className="space-y-4">
           {/* Tabs */}
           <div className="flex border-b border-tertiary">
              <button 
                className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${activeTab === 'content' ? 'border-primary text-primary' : 'border-transparent text-tertiary'}`}
                onClick={() => setActiveTab('content')}
              >
                {t('admin.news.contentTab')}
              </button>
              <button 
                className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${activeTab === 'seo' ? 'border-primary text-primary' : 'border-transparent text-tertiary'}`}
                onClick={() => setActiveTab('seo')}
              >
                {t('admin.news.seoTab')}
              </button>
           </div>

           <form onSubmit={handleSubmit} className="space-y-4">
             {activeTab === 'content' ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-1">{t('common.fields.title')} <span className="text-error">*</span></label>
                    <input
                      type="text"
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                      value={formData.title}
                      onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                      required
                    />
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium mb-1">{t('common.fields.author')}</label>
                    <input
                      type="text"
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                      value={formData.author}
                      onChange={(e) => setFormData({ ...formData, author: e.target.value })}
                    />
                  </div>

                  <div>
                     <label className="block text-sm font-medium mb-1">{t('admin.news.thumbnailUrl')}</label>
                     <input
                        type="text"
                        className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                        value={formData.thumbnail}
                        onChange={(e) => setFormData({ ...formData, thumbnail: e.target.value })}
                        placeholder="https://..."
                     />
                  </div>

                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-1">{t('admin.news.summary')}</label>
                    <textarea
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20 h-20"
                      value={formData.summary}
                      onChange={(e) => setFormData({ ...formData, summary: e.target.value })}
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-1">{t('admin.news.content')} <span className="text-error">*</span></label>
                    <Suspense fallback={<div className="h-64 bg-secondary border border-tertiary rounded-lg animate-pulse" />}>
                      <RichTextEditor
                        value={formData.content}
                        onChange={(val) => setFormData({ ...formData, content: val })}
                        placeholder={t('admin.news.contentPlaceholder', 'Nhập nội dung bài viết...')}
                        minHeight="250px"
                      />
                    </Suspense>
                  </div>

                  <div className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      id="isActive"
                      className="w-4 h-4 rounded border-tertiary"
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    />
                    <label htmlFor="isActive" className="text-sm font-medium cursor-pointer">{t('admin.status.published')}</label>
                  </div>
                </div>
             ) : (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">{t('admin.news.slug')}</label>
                    <input
                      type="text"
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                      value={formData.slug}
                      onChange={(e) => setFormData({ ...formData, slug: e.target.value.toLowerCase().replace(/\s+/g, '-') })}
                      placeholder="post-title-slug"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">{t('admin.news.metaTitle')}</label>
                    <input
                      type="text"
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                      value={formData.metaTitle}
                      onChange={(e) => setFormData({ ...formData, metaTitle: e.target.value })}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">{t('admin.news.metaDescription')}</label>
                    <textarea
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20 h-24"
                      value={formData.metaDescription}
                      onChange={(e) => setFormData({ ...formData, metaDescription: e.target.value })}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">{t('admin.news.metaKeywords')}</label>
                    <input
                      type="text"
                      className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
                      value={formData.metaKeywords}
                      onChange={(e) => setFormData({ ...formData, metaKeywords: e.target.value })}
                      placeholder="news, shop, organic"
                    />
                  </div>
                </div>
             )}
           </form>
        </div>
      </Modal>

      <ConfirmDialog
        isOpen={!!newsToDelete}
        onClose={() => setNewsToDelete(null)}
        onConfirm={() => newsToDelete && deleteMutation.mutate(newsToDelete.code)}
        title={t('common.actions.delete')}
        message={t('messages.confirmDelete')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!itemsToDelete}
        onClose={() => setItemsToDelete(null)}
        onConfirm={() => {
            if (itemsToDelete) {
                bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
            }
        }}
        title={t('common.actions.delete')}
        message={t('messages.confirmDeleteCount', { count: itemsToDelete?.length || 0 })}
        variant="danger"
        isLoading={bulkDeleteMutation.isPending}
        confirmText={t('common.delete')}
        cancelText={t('common.cancel')}
      />
    </div>
  );
};

export default NewsPage;
