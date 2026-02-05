import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Star, Check, X, MessageSquare, Trash2, Search } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { DataTable } from '@/components/common';
import { formatDate } from '@/utils/format';
import { reviewService, type ReviewDto } from '@/services/reviewService';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { PaginationDefaults } from '@/constants';
import { useToast } from '@/store';

export const ReviewsPage = () => {
  const { t } = useTranslation();
  const { success, error: toastError } = useToast();
  const queryClient = useQueryClient();

  // State
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [selectedReview, setSelectedReview] = useState<ReviewDto | null>(null);
  const [reviewToDelete, setReviewToDelete] = useState<ReviewDto | null>(null);
  const [replyText, setReplyText] = useState('');

  // Queries
  const { data: reviewsData, isLoading, isFetching } = useQuery({
    queryKey: ['admin-reviews', currentPage, pageSize, searchQuery, statusFilter],
    queryFn: () => reviewService.search({
      pageIndex: currentPage,
      pageSize: pageSize,
      search: searchQuery,
      filters: statusFilter !== 'all' ? [
        { field: 'isApproved', value: statusFilter === 'approved' }
      ] : []
    }),
  });

  const reviews = reviewsData?.data?.items || [];
  const totalPages = reviewsData?.data?.totalPages || 1;

  // Mutations
  const approveMutation = useMutation({
    mutationFn: (code: string) => reviewService.approve(code),
    onSuccess: () => {
      success(t('messages.updateSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-reviews'] });
    },
    onError: () => toastError(t('messages.updateError'))
  });

  const rejectMutation = useMutation({
    mutationFn: (code: string) => reviewService.reject(code),
    onSuccess: () => {
      success(t('messages.updateSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-reviews'] });
    },
    onError: () => toastError(t('messages.updateError'))
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => reviewService.delete(code),
    onSuccess: () => {
      success(t('messages.deleteSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-reviews'] });
      setReviewToDelete(null);
    },
    onError: () => toastError(t('messages.deleteError'))
  });

  const replyMutation = useMutation({
    mutationFn: (data: { code: string, reply: string }) => 
      reviewService.update(data.code, { adminReply: data.reply }),
    onSuccess: () => {
      success(t('messages.updateSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-reviews'] });
      setSelectedReview(null);
      setReplyText('');
    },
    onError: () => toastError(t('messages.updateError'))
  });

  // Handlers
  const handleReply = () => {
    if (selectedReview && replyText.trim()) {
      replyMutation.mutate({ code: selectedReview.code, reply: replyText });
    }
  };

  const renderStars = (rating: number) => {
    return (
      <div className="flex gap-0.5">
        {Array.from({ length: 5 }).map((_, i) => (
          <Star
            key={i}
            size={14}
            className={i < rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
          />
        ))}
      </div>
    );
  };

  // Columns
  const columns = [
    {
      id: 'product',
      header: t('common.fields.product'),
      accessor: (row: ReviewDto) => (
        <div className="max-w-[200px]">
          <p className="font-medium truncate">{row.productName || row.productCode}</p>
          <p className="text-xs text-tertiary">{row.productCode}</p>
        </div>
      )
    },
    {
      id: 'user',
      header: t('common.fields.user'),
      accessor: (row: ReviewDto) => (
        <div>
          <p className="font-medium">{row.userName}</p>
          <p className="text-xs text-secondary">{row.userCode}</p>
        </div>
      )
    },
    {
      id: 'rating',
      header: t('common.fields.rating'),
      accessor: (row: ReviewDto) => renderStars(row.rating),
      className: 'text-center'
    },
    {
      id: 'comment',
      header: t('common.fields.comment'),
      accessor: (row: ReviewDto) => (
        <div className="max-w-[300px]">
          <p className="text-sm line-clamp-2">{row.comment}</p>
          {row.adminReply && (
            <p className="text-xs text-primary mt-1 flex items-center gap-1 italic">
              <MessageSquare size={10} /> {t('admin.reviews.replied')}
            </p>
          )}
        </div>
      )
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (row: ReviewDto) => (
        <Badge color={row.isApproved ? 'success' : 'warning'} size="sm">
          {row.isApproved ? t('admin.reviews.approved') : t('admin.reviews.pending')}
        </Badge>
      ),
      className: 'text-center'
    },
    {
      id: 'date',
      header: t('common.fields.date'),
      accessor: (row: ReviewDto) => <span className="text-secondary text-sm">{formatDate(row.createdAt)}</span>
    }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.reviews"
        subtitle="admin.subtitles.reviews"
      />

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between bg-primary p-4 rounded-xl border border-tertiary">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" size={18} />
          <input
            type="text"
            placeholder={t('common.placeholders.search')}
            className="w-full pl-10 pr-4 py-2 bg-secondary border border-tertiary rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="flex gap-2">
           <select 
            className="bg-secondary border border-tertiary rounded-lg px-3 py-2 text-sm focus:outline-none"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
           >
              <option value="all">{t('admin.reviews.all')}</option>
              <option value="approved">{t('admin.reviews.approved')}</option>
              <option value="pending">{t('admin.reviews.pending')}</option>
           </select>
           <Button variant="outline" onClick={() => queryClient.invalidateQueries({ queryKey: ['admin-reviews'] })}>
              {t('common.refresh')}
           </Button>
        </div>
      </div>

      <DataTable
        columns={columns}
        data={reviews}
        isLoading={isLoading || isFetching}
        keyField="code"
        
        currentPage={currentPage}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={setPageSize}

        renderRowActions={(row) => (
          <div className="flex items-center gap-1">
            {!row.isApproved && (
              <button
                className="p-1.5 hover:bg-success/10 rounded text-success"
                title={t('admin.actions.approve')}
                onClick={() => approveMutation.mutate(row.code)}
              >
                <Check size={18} />
              </button>
            )}
            {row.isApproved && (
               <button
                 className="p-1.5 hover:bg-warning/10 rounded text-warning"
                 title={t('admin.actions.unapprove')}
                 onClick={() => rejectMutation.mutate(row.code)}
               >
                 <X size={18} />
               </button>
            )}
            <button
              className="p-1.5 hover:bg-primary/10 rounded text-primary"
              title={t('admin.actions.reply')}
              onClick={() => {
                setSelectedReview(row);
                setReplyText(row.adminReply || '');
              }}
            >
              <MessageSquare size={18} />
            </button>
            <button
              className="p-1.5 hover:bg-error/10 rounded text-error"
              title={t('common.actions.delete')}
              onClick={() => setReviewToDelete(row)}
            >
              <Trash2 size={18} />
            </button>
          </div>
        )}
      />

      {/* Reply Modal */}
      <Modal
        isOpen={!!selectedReview}
        onClose={() => setSelectedReview(null)}
        title={t('admin.reviews.replyTitle')}
        footer={
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setSelectedReview(null)}>{t('common.cancel')}</Button>
            <Button onClick={handleReply} isLoading={replyMutation.isPending}>{t('common.save')}</Button>
          </div>
        }
      >
        {selectedReview && (
          <div className="space-y-4">
            <div className="bg-secondary p-4 rounded-lg">
              <div className="flex justify-between items-start mb-2">
                <span className="font-bold">{selectedReview.userName}</span>
                {renderStars(selectedReview.rating)}
              </div>
              <p className="text-sm text-secondary italic">&quot;{selectedReview.comment}&quot;</p>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('admin.reviews.yourReply')}</label>
              <textarea
                className="w-full p-3 bg-primary border rounded-lg h-32 focus:ring-2 focus:ring-primary/20 outline-none"
                placeholder={t('admin.reviews.replyPlaceholder')}
                value={replyText}
                onChange={(e) => setReplyText(e.target.value)}
              />
            </div>
          </div>
        )}
      </Modal>

      <ConfirmDialog
        isOpen={!!reviewToDelete}
        onClose={() => setReviewToDelete(null)}
        onConfirm={() => reviewToDelete && deleteMutation.mutate(reviewToDelete.code)}
        title={t('common.actions.delete')}
        message={t('messages.confirmDelete')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default ReviewsPage;
