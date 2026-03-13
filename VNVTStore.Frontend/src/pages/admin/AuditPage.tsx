import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { useQuery } from '@tanstack/react-query';
import { auditService, AuditLog } from '@/services/auditService';
import { Badge } from '@/components/ui';

const AdminAuditPage = () => {
  const { t } = useTranslation();
  const [pageIndex, setPageIndex] = React.useState(1);
  const [pageSize, setPageSize] = React.useState(20);
  const [filters, setFilters] = React.useState<Record<string, string>>({});

  const { data: response, isLoading } = useQuery({
    queryKey: ['audit-logs', pageIndex, pageSize, filters],
    queryFn: () => auditService.getLogs({ pageIndex, pageSize, ...filters }),
  });

  const logs = response?.data?.items || [];
  const totalCount = response?.data?.totalItems || 0;

  const columns = useMemo<DataTableColumn<AuditLog>[]>(() => [
    {
      id: 'timestamp',
      header: t('auditLog.createdAt'),
      accessor: (row) => new Date(row.timestamp).toLocaleString(),
      sortable: true,
      width: '180px'
    },
    {
      id: 'user',
      header: t('auditLog.user'),
      accessor: 'user',
      sortable: true,
      width: '120px'
    },
    {
      id: 'action',
      header: t('auditLog.action'),
      accessor: (row) => (
        <Badge color={row.action === 'Delete' ? 'error' : row.action === 'Create' ? 'success' : 'info'} variant="soft">
          {row.action}
        </Badge>
      ),
      sortable: true,
      width: '100px'
    },
    {
      id: 'module',
      header: t('auditLog.module'),
      accessor: 'module',
      sortable: true,
      width: '120px'
    },
    {
        id: 'details',
        header: t('auditLog.details'),
        accessor: 'details'
    },
    {
        id: 'status',
        header: t('auditLog.status'),
        accessor: (row) => (
            <span className={row.status === 'success' ? 'text-green-600' : 'text-red-600'}>
                {row.status}
            </span>
        ),
        width: '100px'
    }
  ], [t]);

  const filterDefs = useMemo(() => [
    { id: 'module', label: t('auditLog.module'), type: 'text' as const, placeholder: 'Product, Order...' },
    { id: 'action', label: t('auditLog.action'), type: 'text' as const, placeholder: 'Create, Update...' },
    { id: 'user', label: t('auditLog.user'), type: 'text' as const, placeholder: 'admin' },
  ], [t]);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={t('auditLog.title')}
        subtitle={t('admin.subtitles.auditLogs')}
      />

      <div className="bg-primary rounded-xl shadow-sm border border-border overflow-hidden">
        <DataTable
          columns={columns}
          data={logs}
          keyField="id"
          isLoading={isLoading}
          searchPlaceholder="Tìm kiếm nhật ký..."
          currentPage={pageIndex}
          totalItems={totalCount}
          totalPages={response?.data?.totalPages || 1}
          pageSize={pageSize}
          onPageChange={setPageIndex}
          onPageSizeChange={setPageSize}
          advancedFilterDefs={filterDefs}
          onAdvancedSearch={(newFilters) => {
              setFilters(newFilters);
              setPageIndex(1);
          }}
        />
      </div>
    </div>
  );
};

export default AdminAuditPage;
