import { useTranslation } from 'react-i18next';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { useQuery } from '@tanstack/react-query';
import { paymentService, PaymentTransaction } from '@/services/paymentService';
import { Badge } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { useMemo } from 'react';

const AdminPaymentsPage = () => {
  const { t } = useTranslation();

  const { data: response, isLoading } = useQuery({
    queryKey: ['admin-payments'],
    queryFn: () => paymentService.getAll({ page: 1, limit: 20 }),
  });

  const transactions = response?.data?.items || [];

  const columns = useMemo<DataTableColumn<PaymentTransaction>[]>(() => [
    {
      id: 'orderCode',
      header: t('common.fields.orderCode'),
      accessor: 'orderCode',
      sortable: true,
      width: '140px'
    },
    {
      id: 'amount',
      header: t('admin.payments.amount'),
      accessor: (row) => <span className="font-medium text-emerald-600">{formatCurrency(row.amount)}</span>,
      sortable: true,
      width: '140px'
    },
    {
      id: 'paymentMethod',
      header: t('admin.payments.method'),
      accessor: (row) => (
        <Badge variant="outline" color="secondary">
          {row.paymentMethod}
        </Badge>
      ),
      sortable: true,
      width: '120px'
    },
    {
        id: 'transactionId',
        header: t('admin.payments.transactionId'),
        accessor: 'transactionId',
        width: '180px'
    },
    {
        id: 'userName',
        header: t('admin.audit.user'),
        accessor: 'userName'
    },
    {
        id: 'status',
        header: t('admin.payments.status'),
        accessor: (row) => {
            const colors: Record<string, 'success' | 'warning' | 'error' | 'default'> = {
                success: 'success',
                pending: 'warning',
                failed: 'error',
                refunded: 'default'
            };
            return (
                <Badge color={colors[row.status] || 'default'} variant="soft">
                    {t(`common.status.${row.status}`)}
                </Badge>
            );
        },
        width: '120px'
    },
    {
        id: 'createdAt',
        header: t('common.fields.createdAt'),
        accessor: (row) => new Date(row.createdAt).toLocaleDateString('vi-VN'),
        width: '120px'
    }
  ], [t]);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={t('admin.payments.title')}
        subtitle={t('admin.payments.subtitle')}
      />

      <div className="bg-primary rounded-xl shadow-sm border border-border overflow-hidden">
        <DataTable
          columns={columns}
          data={transactions}
          keyField="id"
          isLoading={isLoading}
          searchPlaceholder="Tìm kiếm giao dịch..."
        />
      </div>
    </div>
  );
};

export default AdminPaymentsPage;
