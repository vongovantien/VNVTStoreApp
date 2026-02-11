import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Search, Trash2, Edit, RefreshCw } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { DataTable, CommonColumns } from '@/components/common';
import { formatDate } from '@/utils/format';
import { couponService, type CouponDto, type CreateCouponRequest, type UpdateCouponRequest } from '@/services/couponService';
import { promotionService } from '@/services/promotionService';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { PaginationDefaults } from '@/constants';
import { COUPON_LIST_FIELDS } from '@/constants/fieldConstants';
import { useToast } from '@/store';

export const CouponsPage = () => {
  const { t } = useTranslation();
  const { success, error: toastError } = useToast();
  const queryClient = useQueryClient();

  // State
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCoupon, setEditingCoupon] = useState<CouponDto | null>(null);
  const [couponToDelete, setCouponToDelete] = useState<CouponDto | null>(null);

  // Form State
  const [formData, setFormData] = useState({
    code: '',
    promotionCode: '',
    isActive: true
  });

  // Queries
  const { data: couponsData, isLoading, isFetching } = useQuery({
    queryKey: ['admin-coupons', currentPage, pageSize, searchQuery],
    queryFn: () => couponService.search({
      pageIndex: currentPage,
      pageSize: pageSize,
      search: searchQuery,
      fields: COUPON_LIST_FIELDS
    }),
  });

  const { data: promotionsData } = useQuery({
    queryKey: ['admin-promotions-all'],
    queryFn: () => promotionService.getAll({ pageSize: 100 }),
  });

  const coupons = couponsData?.data?.items || [];
  const totalPages = couponsData?.data?.totalPages || 1;
  const promotions = promotionsData?.data?.items || [];

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: Partial<CouponDto>) => couponService.create(data as CreateCouponRequest),
    onSuccess: () => {
      success(t('messages.createSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-coupons'] });
      setIsModalOpen(false);
      resetForm();
    },
    onError: (err: Error) => toastError(err.message || t('messages.createError'))
  });

  const updateMutation = useMutation({
    mutationFn: ({ code, data }: { code: string, data: Partial<CouponDto> }) => couponService.update(code, data as UpdateCouponRequest),
    onSuccess: () => {
      success(t('messages.updateSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-coupons'] });
      setIsModalOpen(false);
      resetForm();
    },
    onError: (err: Error) => toastError(err.message || t('messages.updateError'))
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => couponService.delete(code),
    onSuccess: () => {
      success(t('messages.deleteSuccess'));
      queryClient.invalidateQueries({ queryKey: ['admin-coupons'] });
      setCouponToDelete(null);
    },
    onError: () => toastError(t('messages.deleteError'))
  });

  // Handlers
  const resetForm = () => {
    setFormData({ code: '', promotionCode: '', isActive: true });
    setEditingCoupon(null);
  };

  const handleEdit = (coupon: CouponDto) => {
    setEditingCoupon(coupon);
    setFormData({
      code: coupon.code,
      promotionCode: coupon.promotionCode || '',
      isActive: coupon.isActive
    });
    setIsModalOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingCoupon) {
      updateMutation.mutate({ code: editingCoupon.code, data: { promotionCode: formData.promotionCode, isActive: formData.isActive } });
    } else {
      createMutation.mutate(formData);
    }
  };

  const generateRandomCode = () => {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = '';
    for (let i = 0; i < 8; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    setFormData({ ...formData, code: result });
  };

  // Columns
  const columns = [
    {
      id: 'code',
      header: t('common.fields.code'),
      accessor: (row: CouponDto) => <span className="font-mono font-bold text-primary">{row.code}</span>,
    },
    {
      id: 'promotion',
      header: t('admin.promotion.name'),
      accessor: (row: CouponDto) => (
        <div>
          <p className="font-medium">{row.promotionName || row.promotionCode || t('common.none')}</p>
          <p className="text-xs text-tertiary">{row.promotionCode}</p>
        </div>
      )
    },
    {
      id: 'usage',
      header: t('admin.coupon.usage'),
      accessor: (row: CouponDto) => (
        <div className="flex flex-col items-center">
          <span className="text-sm font-medium">{row.usageCount || 0}</span>
          <p className="text-[10px] text-tertiary uppercase tracking-wider">{t('admin.coupon.timesUsed')}</p>
        </div>
      ),
      className: 'text-center'
    },
    CommonColumns.createStatusColumn(t),
    {
      id: 'date',
      header: t('common.fields.date'),
      accessor: (row: CouponDto) => <span className="text-secondary text-sm">{formatDate(row.createdAt)}</span>
    }
  ];

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.coupons"
        subtitle="admin.subtitles.coupons"
        rightSection={
          <Button leftIcon={<Plus size={18} />} onClick={() => { resetForm(); setIsModalOpen(true); }}>
            {t('admin.coupon.add')}
          </Button>
        }
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
           <Button variant="outline" onClick={() => queryClient.invalidateQueries({ queryKey: ['admin-coupons'] })}>
              <RefreshCw size={18} className={isFetching ? 'animate-spin' : ''} />
           </Button>
        </div>
      </div>

      <DataTable
        columns={columns}
        data={coupons}
        isLoading={isLoading || isFetching}
        keyField="code"
        
        currentPage={currentPage}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={setPageSize}

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
              onClick={() => setCouponToDelete(row)}
            >
              <Trash2 size={18} />
            </button>
          </div>
        )}
      />

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingCoupon ? t('admin.coupon.edit') : t('admin.coupon.add')}
        footer={
          <div className="flex justify-end gap-2">
             <Button variant="ghost" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
             <Button onClick={handleSubmit} isLoading={createMutation.isPending || updateMutation.isPending}>{t('common.save')}</Button>
          </div>
        }
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('common.fields.code')} <span className="text-error">*</span></label>
            <div className="flex gap-2">
              <input
                type="text"
                className="flex-1 p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20 disabled:opacity-50 font-mono"
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                disabled={!!editingCoupon}
                required
              />
              {!editingCoupon && (
                <Button type="button" variant="outline" size="sm" onClick={generateRandomCode}>
                  {t('common.generate')}
                </Button>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">{t('admin.promotion.name')} <span className="text-error">*</span></label>
            <select
              className="w-full p-2 bg-secondary border border-tertiary rounded-lg outline-none focus:ring-2 focus:ring-primary/20"
              value={formData.promotionCode}
              onChange={(e) => setFormData({ ...formData, promotionCode: e.target.value })}
              required
            >
              <option value="">{t('common.placeholders.select')}</option>
              {promotions.map(p => (
                <option key={p.code} value={p.code}>{p.name} ({p.discountType === 'PERCENTAGE' ? `${p.discountValue}%` : t('common.currency', { value: p.discountValue })})</option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-2 pt-2">
            <input
              type="checkbox"
              id="isActive"
              className="w-4 h-4 rounded border-tertiary"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
            />
            <label htmlFor="isActive" className="text-sm font-medium cursor-pointer">{t('admin.status.active')}</label>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={!!couponToDelete}
        onClose={() => setCouponToDelete(null)}
        onConfirm={() => couponToDelete && deleteMutation.mutate(couponToDelete.code)}
        title={t('common.actions.delete')}
        message={t('messages.confirmDelete')}
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default CouponsPage;
